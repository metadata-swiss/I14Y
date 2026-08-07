using System.Text.RegularExpressions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using LuceneCatalog = Bfs.Iop.Core.Lucene.LuceneFields;

namespace Bfs.Iop.Core.Elasticsearch;

/// <summary>
/// Builds the Elasticsearch query/aggregation bodies, reproducing the Lucene ranking rules:
/// per-field boosts (Title/Name 2.0, Keyword 1.75, Description/Identifier 1.5), a partial-match
/// ngram clause dampened to 0.75, a function_score combining the registration-status score
/// multiplier (applied only when a text query is present) with the "most reused concept" score
/// multiplier (applied to Concept documents regardless of whether there is a text query — see
/// <see cref="WrapWithRelevanceBoosts"/>), facet + numeric filters, and the publication-level/agency
/// authorization filter.
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
        // PoC simplification of Lucene's DrillSideways: aggregate over the text query + authorization
        // only (not the facet selections), so every facet reports counts as if its own selection were
        // removed. Full per-dimension drill-sideways is a follow-up.
        return new Dictionary<string, object?>
        {
            ["size"] = 0,
            ["track_total_hits"] = true,
            ["query"] = BuildQuery(queryString, languages, filter: null, role, agencies),
            ["aggs"] = BuildAggregations(),
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

        var scored = WrapWithRelevanceBoosts(textQuery, hasText);

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

    // Combines the registration-status boost with the "most reused concept" boost (see
    // CatalogDocumentFactory.ReuseCountToWeight) into a single function_score. The reuse factor is
    // scoped to Concept documents only (via its own filter) so it never changes the relative order of
    // non-Concept resource types; it applies even without a search term, since a browse/filter-only
    // listing of concepts should still surface the most reused ones first. The registration-status
    // factor keeps its existing behaviour: only applied when there is an actual search term.
    private static object WrapWithRelevanceBoosts(object innerQuery, bool hasText)
    {
        var functions = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["filter"] = new Dictionary<string, object?>
                {
                    ["term"] = new Dictionary<string, object?> { [EsCatalogFields.Type] = nameof(SearchResourceType.Concept) },
                },
                // field_value_factor value = factor * reuseWeight = 0.01 * weight = weight/100.
                ["field_value_factor"] = new Dictionary<string, object?>
                {
                    ["field"] = EsCatalogFields.ReuseWeight,
                    ["factor"] = 0.01,
                    ["missing"] = 100,
                },
            },
        };

        if (hasText)
        {
            functions.Add(new Dictionary<string, object?>
            {
                // field_value_factor value = factor * registrationStatusWeight = 0.01 * weight = weight/100.
                ["field_value_factor"] = new Dictionary<string, object?>
                {
                    ["field"] = EsCatalogFields.RegistrationStatusWeight,
                    ["factor"] = 0.01,
                    ["missing"] = 100,
                },
            });
        }

        return new Dictionary<string, object?>
        {
            ["function_score"] = new Dictionary<string, object?>
            {
                ["query"] = innerQuery,
                ["functions"] = functions,
                ["score_mode"] = "multiply",
                ["boost_mode"] = "multiply",
            },
        };
    }

    private static void AddFilters(List<object> filters, CatalogSearchFilter filter)
    {
        AddTerms(filters, EsCatalogFields.AccessRights, filter.AccessRights);
        AddTerms(filters, EsCatalogFields.BusinessEvents, filter.BusinessEvents);
        AddTerms(filters, EsCatalogFields.Formats, filter.Formats);
        AddTerms(filters, EsCatalogFields.LifeEvents, filter.LifeEvents);
        AddTerms(filters, EsCatalogFields.Themes, filter.Themes);
        AddTerms(filters, EsCatalogFields.PublisherIdentifier, filter.PublisherIdentifiers.Select(x => x.ToLowerInvariant()));
        AddTerms(filters, EsCatalogFields.Type, filter.Types.Select(x => x.ToString()));

        AddTerms(filters, EsCatalogFields.RegistrationStatus, filter.RegistrationStatuses.Select(x => (int)x));
        AddTerms(filters, EsCatalogFields.RegistrationStatusProposal, filter.RegistrationStatusProposals.Select(x => (int)x));
        AddTerms(filters, EsCatalogFields.PublicationLevel, filter.PublicationLevels.Select(x => (int)x));
        AddTerms(filters, EsCatalogFields.PublicationLevelProposal, filter.PublicationLevelProposals.Select(x => (int)x));
        AddTerms(filters, EsCatalogFields.ConceptType, filter.ConceptValueTypes.Select(x => (int)x));

        if (filter.Structure.HasValue)
        {
            filters.Add(new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?>
                {
                    [EsCatalogFields.HasStructure] = filter.Structure.Value is SearchStructureOption.WithStructure,
                },
            });
        }
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

    private static void AddTerms<T>(List<object> filters, string field, IEnumerable<T> values)
    {
        var array = values.Cast<object>().ToArray();
        if (array.Length == 0)
        {
            return;
        }

        filters.Add(new Dictionary<string, object?>
        {
            ["terms"] = new Dictionary<string, object?> { [field] = array },
        });
    }

    // Aggregations keyed by the same dimension identifiers the Lucene count uses, so the existing
    // count command handler mapping keeps working. Numeric dimensions bucket on the integer value.
    private static Dictionary<string, object?> BuildAggregations() => new()
    {
        [LuceneCatalog.Catalog.AccessRights] = TermsAgg(EsCatalogFields.AccessRights),
        [LuceneCatalog.Catalog.BusinessEvents] = TermsAgg(EsCatalogFields.BusinessEvents),
        [LuceneCatalog.Catalog.Formats] = TermsAgg(EsCatalogFields.Formats),
        [LuceneCatalog.Catalog.LifeEvents] = TermsAgg(EsCatalogFields.LifeEvents),
        [LuceneCatalog.Catalog.Themes] = TermsAgg(EsCatalogFields.Themes),
        [LuceneCatalog.Catalog.PublisherIdentifier] = TermsAgg(EsCatalogFields.PublisherIdentifier),
        [LuceneCatalog.Catalog.Type] = TermsAgg(EsCatalogFields.Type),
        [LuceneCatalog.Catalog.RegistrationStatus] = TermsAgg(EsCatalogFields.RegistrationStatus),
        [LuceneCatalog.Catalog.RegistrationStatusProposal] = TermsAgg(EsCatalogFields.RegistrationStatusProposal),
        [LuceneCatalog.Catalog.PublicationLevel] = TermsAgg(EsCatalogFields.PublicationLevel),
        [LuceneCatalog.Catalog.PublicationLevelProposal] = TermsAgg(EsCatalogFields.PublicationLevelProposal),
        [LuceneCatalog.Catalog.ConceptType] = TermsAgg(EsCatalogFields.ConceptType),
    };

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
