using System.Text.RegularExpressions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Search.Abstractions;


namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Builds the Elasticsearch query/aggregation bodies. The ranking rules are:
/// per-field boosts (Title/Name 2.0, Keyword 1.75, Description/Identifier 1.5), a partial-match
/// ngram clause dampened to 0.75, the registration-status score multiplier (function_score /
/// field_value_factor, applied only when a text query is present), facet + numeric filters, and
/// the publication-level/agency authorization filter.
/// </summary>
internal static partial class CatalogQueryBuilder
{
    private const float TitleBoost = 2.0f;
    private const float KeywordBoost = 1.75f;
    private const float DescriptionBoost = 1.5f;
    private const float IdentifierBoost = 1.5f;
    private const float PartialMatchFactor = 0.75f;

    [GeneratedRegex(@"^[^\s@""]+@[^\s@""]+\.[^\s@""]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public static Dictionary<string, object?> BuildSearchBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        BusinessRole role,
        IReadOnlyList<string> agencies,
        int from,
        int size)
    {
        return new Dictionary<string, object?>
        {
            ["from"] = from,
            ["size"] = size,
            ["track_total_hits"] = true,
            ["query"] = BuildQuery(queryString, languages, filter, role, agencies),
        };
    }

    public static Dictionary<string, object?> BuildCountBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        BusinessRole role,
        IReadOnlyList<string> agencies)
    {
        // Drill-sideways: each dimension is counted with every facet selection applied EXCEPT its own. That is what makes a facet count a truthful prediction of
        // "click me and you get this many" while still letting the user switch within the dimension
        // they have already drilled into.
        //
        // The base query therefore carries text + authorization only; the per-dimension selections
        // live in a filter aggregation around each terms aggregation.
        var clausesByDimension = BuildFilterClausesByDimension(filter);

        return new Dictionary<string, object?>
        {
            ["size"] = 0,
            ["track_total_hits"] = true,
            ["query"] = BuildQuery(queryString, languages, filter: null, role, agencies),
            ["aggs"] = BuildDrillSidewaysAggregations(clausesByDimension),
        };
    }

    private static Dictionary<string, object?> BuildQuery(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        BusinessRole role,
        IReadOnlyList<string> agencies)
    {
        var hasText = !string.IsNullOrWhiteSpace(queryString);
        var langs = languages.Count > 0 ? languages : EsCatalogFields.Languages;

        object textQuery = !hasText
            ? MatchAll()
            : TryBuildEmailQuery(queryString!) ?? BuildFreeTextQuery(queryString!, langs);

        // Registration-status multiplier only applies when there is an actual search term.
        var scored = hasText ? WrapWithRegistrationStatusBoost(textQuery) : textQuery;

        var filters = new List<object>();
        if (filter is not null)
        {
            AddFilters(filters, filter);
        }

        AddAuthorizationFilter(filters, role, agencies);

        var boolQuery = new Dictionary<string, object?> { ["must"] = new[] { scored } };
        if (filters.Count > 0)
        {
            boolQuery["filter"] = filters;
        }

        return new Dictionary<string, object?> { ["bool"] = boolQuery };
    }

    private static object BuildFreeTextQuery(string queryString, IReadOnlyList<string> languages)
    {
        var exactFields = new List<string>();
        var ngramFields = new List<string>();

        foreach (var lang in languages)
        {
            exactFields.Add(Boosted($"{EsCatalogFields.Title}.{lang}", TitleBoost));
            exactFields.Add(Boosted($"{EsCatalogFields.Name}.{lang}", TitleBoost));
            exactFields.Add(Boosted($"{EsCatalogFields.Keyword}.{lang}", KeywordBoost));
            exactFields.Add(Boosted($"{EsCatalogFields.Description}.{lang}", DescriptionBoost));
            exactFields.Add($"{EsCatalogFields.ContactPointFn}.{lang}");
            exactFields.Add($"{EsCatalogFields.ContactPointHasAddress}.{lang}");
            exactFields.Add($"{EsCatalogFields.ContactPointNote}.{lang}");

            ngramFields.Add(Boosted($"{EsCatalogFields.Title}.{lang}.ngram", TitleBoost));
            ngramFields.Add(Boosted($"{EsCatalogFields.Name}.{lang}.ngram", TitleBoost));
            ngramFields.Add(Boosted($"{EsCatalogFields.Keyword}.{lang}.ngram", KeywordBoost));
            ngramFields.Add(Boosted($"{EsCatalogFields.Description}.{lang}.ngram", DescriptionBoost));
        }

        // Non-language text properties.
        exactFields.Add(Boosted(EsCatalogFields.Identifier, IdentifierBoost));
        exactFields.Add(EsCatalogFields.DataOwner);
        exactFields.Add(EsCatalogFields.ResponsiblePersonName);
        exactFields.Add(EsCatalogFields.ResponsibleDeputyName);

        var exact = new Dictionary<string, object?>
        {
            ["multi_match"] = new Dictionary<string, object?>
            {
                ["query"] = queryString,
                ["type"] = "best_fields",
                ["fields"] = exactFields.ToArray(),
            },
        };

        // Partial/substring matches via the ngram sub-fields, dampened (mirrors PartialMatchCorrectionFactor 0.75).
        var partial = new Dictionary<string, object?>
        {
            ["multi_match"] = new Dictionary<string, object?>
            {
                ["query"] = queryString,
                ["type"] = "best_fields",
                ["fields"] = ngramFields.ToArray(),
                ["boost"] = PartialMatchFactor,
            },
        };

        return new Dictionary<string, object?>
        {
            ["bool"] = new Dictionary<string, object?>
            {
                ["should"] = new object[] { exact, partial },
                ["minimum_should_match"] = 1,
            },
        };
    }

    // A query that is a single email address matches the exact keyword email fields only (bug #725).
    private static object? TryBuildEmailQuery(string queryString)
    {
        var trimmed = queryString.Trim().Trim('"').Trim();
        if (!EmailRegex().IsMatch(trimmed))
        {
            return null;
        }

        var term = trimmed.ToLowerInvariant();
        var should = EsCatalogFields.EmailFields
            .Select(field => (object)new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?> { [field] = term },
            })
            .ToArray();

        return new Dictionary<string, object?>
        {
            ["bool"] = new Dictionary<string, object?> { ["should"] = should, ["minimum_should_match"] = 1 },
        };
    }

    private static object WrapWithRegistrationStatusBoost(object innerQuery) => new Dictionary<string, object?>
    {
        ["function_score"] = new Dictionary<string, object?>
        {
            ["query"] = innerQuery,
            // field_value_factor value = factor * registrationStatusWeight = 0.01 * weight = weight/100.
            ["field_value_factor"] = new Dictionary<string, object?>
            {
                ["field"] = EsCatalogFields.RegistrationStatusWeight,
                ["factor"] = 0.01,
                ["missing"] = 100,
            },
            ["boost_mode"] = "multiply",
        },
    };

    private static void AddFilters(List<object> filters, CatalogSearchFilter filter)
    {
        // Flattening the same per-dimension map the count path uses keeps search and count from
        // drifting apart: a filter added in one place is applied by both.
        foreach (var clause in BuildFilterClausesByDimension(filter).SelectMany(x => x.Value))
        {
            filters.Add(clause);
        }
    }

    /// <summary>
    /// The facet selections as clauses grouped by the dimension they constrain, so the count path can
    /// leave one dimension out while applying the rest.
    /// </summary>
    /// <remarks>
    /// Dimensions with nothing selected are absent rather than present-and-empty; callers only ever
    /// flatten the values, so an empty entry would add nothing but would still have to be reasoned
    /// about.
    /// </remarks>
    private static Dictionary<string, List<object>> BuildFilterClausesByDimension(CatalogSearchFilter? filter)
    {
        var clauses = new Dictionary<string, List<object>>();

        if (filter is null)
        {
            return clauses;
        }

        AddTermsClause(clauses, CatalogFacetDimensions.AccessRights, EsCatalogFields.AccessRights, filter.AccessRights);
        AddTermsClause(clauses, CatalogFacetDimensions.BusinessEvents, EsCatalogFields.BusinessEvents, filter.BusinessEvents);
        AddTermsClause(clauses, CatalogFacetDimensions.Formats, EsCatalogFields.Formats, filter.Formats);
        AddTermsClause(clauses, CatalogFacetDimensions.LifeEvents, EsCatalogFields.LifeEvents, filter.LifeEvents);
        AddTermsClause(clauses, CatalogFacetDimensions.Themes, EsCatalogFields.Themes, filter.Themes);
        AddTermsClause(clauses, CatalogFacetDimensions.PublisherIdentifier, EsCatalogFields.PublisherIdentifier, filter.PublisherIdentifiers.Select(x => x.ToLowerInvariant()));
        AddTermsClause(clauses, CatalogFacetDimensions.Type, EsCatalogFields.Type, filter.Types.Select(x => x.ToString()));

        AddTermsClause(clauses, CatalogFacetDimensions.RegistrationStatus, EsCatalogFields.RegistrationStatus, filter.RegistrationStatuses.Select(x => (int)x));
        AddTermsClause(clauses, CatalogFacetDimensions.RegistrationStatusProposal, EsCatalogFields.RegistrationStatusProposal, filter.RegistrationStatusProposals.Select(x => (int)x));
        AddTermsClause(clauses, CatalogFacetDimensions.PublicationLevel, EsCatalogFields.PublicationLevel, filter.PublicationLevels.Select(x => (int)x));
        AddTermsClause(clauses, CatalogFacetDimensions.PublicationLevelProposal, EsCatalogFields.PublicationLevelProposal, filter.PublicationLevelProposals.Select(x => (int)x));
        AddTermsClause(clauses, CatalogFacetDimensions.ConceptType, EsCatalogFields.ConceptType, filter.ConceptValueTypes.Select(x => (int)x));

        if (filter.Structure.HasValue)
        {
            clauses[CatalogFacetDimensions.HasStructure] =
            [
                new Dictionary<string, object?>
                {
                    ["term"] = new Dictionary<string, object?>
                    {
                        [EsCatalogFields.HasStructure] = filter.Structure.Value is SearchStructureOption.WithStructure,
                    },
                },
            ];
        }

        return clauses;
    }

    private static void AddTermsClause<T>(
        Dictionary<string, List<object>> clauses,
        string dimension,
        string field,
        IEnumerable<T>? values)
    {
        var list = values?.ToArray() ?? [];

        if (list.Length == 0)
        {
            return;
        }

        clauses[dimension] =
        [
            new Dictionary<string, object?>
            {
                ["terms"] = new Dictionary<string, object?> { [field] = list },
            },
        ];
    }

    private static void AddAuthorizationFilter(List<object> filters, BusinessRole role, IReadOnlyList<string> agencies)
    {
        if (role is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward)
        {
            return;
        }

        var should = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?> { [EsCatalogFields.PublicationLevel] = (int)PublicationLevel.Public },
            },
        };

        if (agencies.Count > 0)
        {
            should.Add(new Dictionary<string, object?>
            {
                ["terms"] = new Dictionary<string, object?>
                {
                    [EsCatalogFields.PublisherIdentifier] = agencies.Select(x => x.ToLowerInvariant()).ToArray(),
                },
            });
        }

        filters.Add(new Dictionary<string, object?>
        {
            ["bool"] = new Dictionary<string, object?> { ["should"] = should, ["minimum_should_match"] = 1 },
        });
    }

    /// <summary>
    /// The facet dimensions, keyed by the identifiers <c>CatalogSearchCountQueryService</c> and the
    /// UI read facets by, and paired with the index field each one buckets on. Numeric dimensions
    /// bucket on the integer value. Every entry must also
    /// appear in <see cref="BuildFilterClausesByDimension"/>, or that dimension's own selection would
    /// not be excluded from its own count and drill-sideways would silently degrade for it.
    /// </summary>
    private static readonly (string Dimension, string Field)[] _facetDimensions =
    [
        (CatalogFacetDimensions.AccessRights, EsCatalogFields.AccessRights),
        (CatalogFacetDimensions.BusinessEvents, EsCatalogFields.BusinessEvents),
        (CatalogFacetDimensions.Formats, EsCatalogFields.Formats),
        (CatalogFacetDimensions.LifeEvents, EsCatalogFields.LifeEvents),
        (CatalogFacetDimensions.Themes, EsCatalogFields.Themes),
        // Buckets on the case-preserving field, NOT the lowercased one the filter and authorization
        // clauses match on: these keys are resolved against agents.identifier case-sensitively.
        (CatalogFacetDimensions.PublisherIdentifier, EsCatalogFields.PublisherIdentifierLabel),
        (CatalogFacetDimensions.Type, EsCatalogFields.Type),
        (CatalogFacetDimensions.RegistrationStatus, EsCatalogFields.RegistrationStatus),
        (CatalogFacetDimensions.RegistrationStatusProposal, EsCatalogFields.RegistrationStatusProposal),
        (CatalogFacetDimensions.PublicationLevel, EsCatalogFields.PublicationLevel),
        (CatalogFacetDimensions.PublicationLevelProposal, EsCatalogFields.PublicationLevelProposal),
        (CatalogFacetDimensions.ConceptType, EsCatalogFields.ConceptType),
        // Without this the Structures filter is always empty in the response, because the count
        // mapping looks the dimension up by name and finds nothing.
        (CatalogFacetDimensions.HasStructure, EsCatalogFields.HasStructure),
    ];

    /// <summary>
    /// Name of the sub-aggregation holding the actual buckets inside each dimension's filter
    /// aggregation. The response parser looks for this exact key.
    /// </summary>
    internal const string FacetValuesAggregationName = "values";

    /// <summary>
    /// One filter aggregation per dimension, each applying every selection except its own, wrapping
    /// the terms aggregation that produces the buckets.
    /// </summary>
    private static Dictionary<string, object?> BuildDrillSidewaysAggregations(
        IReadOnlyDictionary<string, List<object>> clausesByDimension)
    {
        var aggregations = new Dictionary<string, object?>();

        foreach (var (dimension, field) in _facetDimensions)
        {
            var otherClauses = clausesByDimension
                .Where(x => x.Key != dimension)
                .SelectMany(x => x.Value)
                .ToList();

            aggregations[dimension] = new Dictionary<string, object?>
            {
                // A filter aggregation is required even with nothing to filter on: it keeps the
                // response shape identical whether or not selections exist, so the parser has one
                // path instead of two. match_all is free.
                ["filter"] = otherClauses.Count > 0
                    ? new Dictionary<string, object?>
                    {
                        ["bool"] = new Dictionary<string, object?> { ["filter"] = otherClauses },
                    }
                    : MatchAll(),
                ["aggs"] = new Dictionary<string, object?>
                {
                    [FacetValuesAggregationName] = TermsAgg(field),
                },
            };
        }

        return aggregations;
    }

    private static Dictionary<string, object?> TermsAgg(string field) => new()
    {
        ["terms"] = new Dictionary<string, object?> { ["field"] = field, ["size"] = 1000 },
    };

    // Formats a multi_match field^boost token with the invariant culture so a comma-decimal locale
    // (it-IT, de-DE, fr-FR) can't emit an invalid boost like "keyword.de^1,75" that ES would reject.
    private static string Boosted(string field, float boost) => FormattableString.Invariant($"{field}^{boost}");

    private static Dictionary<string, object?> MatchAll() => new()
    {
        ["match_all"] = new Dictionary<string, object?>(),
    };
}
