using System.Text.RegularExpressions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogQueryBuilder
{
    private const int MaxFacetBuckets = 1000;

    internal const int MaxResultWindow = Paging.MaxResultWindow;

    private static readonly Regex _emailQuery = new(
        @"^[^\s@""]+@[^\s@""]+\.[^\s@""]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static Dictionary<string, object?> BuildSearchBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CatalogScoring scoring,
        int from,
        int size) => new()
    {
        ["from"] = from,
        ["size"] = size,
        ["track_total_hits"] = true,
        ["query"] = BuildQuery(queryString, languages, filter, caller, scoring),
        ["sort"] = RelevanceThenStatusThenReuse(scoring),
    };

    public static Dictionary<string, object?> BuildPointInTimeBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CatalogScoring scoring,
        string pointInTimeId,
        TimeSpan keepAlive,
        int size,
        IReadOnlyList<object>? searchAfter)
    {
        var body = new Dictionary<string, object?>
        {
            ["size"] = size,
            ["track_total_hits"] = false,
            ["query"] = BuildQuery(queryString, languages, filter, caller, scoring),
            ["sort"] = RelevanceThenStatusThenReuse(scoring, new Dictionary<string, object?> { ["_shard_doc"] = "asc" }),
            ["pit"] = new Dictionary<string, object?>
            {
                ["id"] = pointInTimeId,
                ["keep_alive"] = $"{Math.Max(1, (int)Math.Ceiling(keepAlive.TotalSeconds))}s",
            },
        };

        if (searchAfter is { Count: > 0 })
        {
            body["search_after"] = searchAfter;
        }

        return body;
    }

    /// <summary>
    ///     How well the text matched, then how settled the resource is, then how much it is reused.
    /// </summary> 
    private static object[] RelevanceThenStatusThenReuse(CatalogScoring scoring, params object[] then) =>
    [
        new Dictionary<string, object?> { ["_score"] = "desc" },
        Descending(EsCatalogFields.RegistrationStatusWeight, scoring.RecordedWeight),
        Descending(EsCatalogFields.ReuseCount, "_last"),
        .. then,
    ];

    /// <summary>
    ///     A resource indexed before the field existed ranks as <paramref name="missing" /> says:
    ///     as though it were Recorded for status, and below everything for reuse.
    /// </summary>
    private static Dictionary<string, object?> Descending(string field, object missing) => new()
    {
        [field] = new Dictionary<string, object?>
        {
            ["order"] = "desc",
            ["missing"] = missing,

            // Sorting on a field the index has no mapping for is a 400, not an empty result, so
            // without this every search fails between deploying and the first rebuild.
            ["unmapped_type"] = "integer",
        },
    };

    public static Dictionary<string, object?> BuildCountBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CatalogScoring scoring) => new()
    {
        ["size"] = 0,
        ["query"] = BuildQuery(queryString, languages, filter: null, caller, scoring),
        ["aggs"] = BuildDrillSidewaysAggregations(filter),
    };

    private static Dictionary<string, object?> BuildQuery(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CatalogScoring scoring)
    {
        var must = new List<object>();
        var filters = new List<object>();

        if (!string.IsNullOrWhiteSpace(queryString))
        {
            must.Add(BuildFreeText(queryString, languages, caller, scoring));
        }

        filters.AddRange(BuildFilterClauses(filter));

        var authorization = BuildAuthorizationClause(caller);

        if (authorization is not null)
        {
            filters.Add(authorization);
        }

        var boolQuery = new Dictionary<string, object?>();

        if (must.Count > 0)
        {
            boolQuery["must"] = must.ToArray();
        }

        if (filters.Count > 0)
        {
            boolQuery["filter"] = filters.ToArray();
        }

        return boolQuery.Count == 0
            ? new Dictionary<string, object?> { ["match_all"] = new Dictionary<string, object?>() }
            : new Dictionary<string, object?> { ["bool"] = boolQuery };
    }

    private static Dictionary<string, object?>? BuildAuthorizationClause(SearchCaller caller) => caller.Role switch
    {
        BusinessRole.SwissDataSteward or BusinessRole.InteroperabilityService => null,
        BusinessRole.StewardshipOrganisationViewer
            or BusinessRole.LocalDataSteward
            or BusinessRole.Submitter => AgencyScoped(caller.Agencies),
        _ => PublicOnly(),
    };


    private static Dictionary<string, object?> PublicOnly() => new()
    {
        ["term"] = new Dictionary<string, object?>
        {
            [EsCatalogFields.PublicationLevel] = PublicationLevel.Public.ToString(),
        },
    };

    private static Dictionary<string, object?> AgencyScoped(IReadOnlyList<string> agencies)
    {
        var should = new List<object> { PublicOnly() };

        var scoped = agencies.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToLowerInvariant()).ToArray();

        if (scoped.Length > 0)
        {
            should.Add(new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?>
                {
                    ["filter"] = new object[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["term"] = new Dictionary<string, object?>
                            {
                                [EsCatalogFields.PublicationLevel] = PublicationLevel.Internal.ToString(),
                            },
                        },
                        new Dictionary<string, object?>
                        {
                            ["terms"] = new Dictionary<string, object?>
                            {
                                [EsCatalogFields.PublisherIdentifier] =
                                    scoped,
                            },
                        },
                    },
                },
            });
        }

        return new Dictionary<string, object?>
        {
            ["bool"] = new Dictionary<string, object?>
            {
                ["should"] = should.ToArray(),
                ["minimum_should_match"] = 1,
            },
        };
    }

    private static Dictionary<string, object?> BuildFreeText(
        string queryString,
        IReadOnlyList<string> languages,
        SearchCaller caller,
        CatalogScoring scoring)
    {
        var trimmed = queryString.Trim().Trim('"').Trim();

        // A signed-in caller searches everything; anyone else only the fields the public API returns.
        bool Searchable(string field) =>
            caller.HasValidToken || !EsCatalogFields.NonPublicFields.Contains(field);

        if (_emailQuery.IsMatch(trimmed))
        {
            return AnyOf([.. EsCatalogFields.EmailFields.Where(Searchable).Select(x => ExactTerm(x, trimmed))]);
        }

        var fields = new List<string>();
        var ngramFields = new List<string>();

        foreach (var field in EsCatalogFields.MultiLanguageFields)
        {
            foreach (var language in languages)
            {
                fields.Add(EsCatalogFields.Weighted(scoring.FieldWeights, field, $"{field}.{language}"));
                ngramFields.Add(EsCatalogFields.Weighted(scoring.FieldWeights, field, $"{field}.{language}.ngram"));
            }
        }

        fields.AddRange(EsCatalogFields.SearchableKeywordFields
            .Where(Searchable)
            .Select(x => EsCatalogFields.Weighted(scoring.FieldWeights, x, EsCatalogFields.TextOf(x))));

        var should = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["multi_match"] = new Dictionary<string, object?>
                {
                    ["query"] = trimmed,
                    ["fields"] = fields.ToArray(),
                    ["type"] = "best_fields",
                },
            },
            new Dictionary<string, object?>
            {
                ["multi_match"] = new Dictionary<string, object?>
                {
                    ["query"] = trimmed,
                    ["fields"] = ngramFields.ToArray(),
                    ["type"] = "best_fields",
                    ["minimum_should_match"] = scoring.PartialMatchMinimumShouldMatch,
                    ["boost"] = scoring.PartialMatchBoost,
                },
            },
        };

        should.Add(ExactTerm(EsCatalogFields.Id, trimmed));

        foreach (var emailField in EsCatalogFields.EmailFields.Where(Searchable))
        {
            should.Add(ExactTerm(emailField, trimmed));
        }

        return AnyOf(should);
    }

    private static Dictionary<string, object?> ExactTerm(string field, string value) => new()
    {
        ["term"] = new Dictionary<string, object?> { [field] = value.ToLowerInvariant() },
    };

    private static Dictionary<string, object?> AnyOf(IReadOnlyList<object> should) => new()
    {
        ["bool"] = new Dictionary<string, object?>
        {
            ["should"] = should.ToArray(),
            ["minimum_should_match"] = 1,
        },
    };

    private static List<object> BuildFilterClauses(CatalogSearchFilter? filter)
    {
        var clauses = new List<object>();

        if (filter is null)
        {
            return clauses;
        }

        AddTerms(clauses, EsCatalogFields.Themes, filter.Themes);
        AddTerms(clauses, EsCatalogFields.AttributedAgentIdentifiers, filter.AttributedAgentIdentifiers);
        AddTerms(clauses, EsCatalogFields.AccessRights, filter.AccessRights);
        AddTerms(clauses, EsCatalogFields.Formats, filter.Formats);
        AddTerms(clauses, EsCatalogFields.Licenses, filter.Licenses);
        AddTerms(clauses, EsCatalogFields.BusinessEvents, filter.BusinessEvents);
        AddTerms(clauses, EsCatalogFields.LifeEvents, filter.LifeEvents);
        AddTerms(clauses, EsCatalogFields.Type, filter.Types.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.ConceptType, filter.ConceptTypes.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.PublicationLevel, filter.PublicationLevels.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.PublicationLevelProposal, filter.PublicationLevelProposals.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.RegistrationStatus, filter.RegistrationStatuses.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.RegistrationStatusProposal, filter.RegistrationStatusProposals.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.PublisherIdentifier,
            filter.PublisherIdentifiers.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToLowerInvariant()));
        AddTerms(clauses, EsCatalogFields.KeywordFilter,
            filter.Keywords.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));

        if (filter.Structure.HasValue)
        {
            clauses.Add(new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?>
                {
                    [EsCatalogFields.HasStructure] = filter.Structure.Value == IndexStructureOption.WithStructure,
                },
            });
        }

        return clauses;
    }

    private static void AddTerms(List<object> clauses, string field, IEnumerable<string?> values)
    {
        var present = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        if (present.Length == 0)
        {
            return;
        }

        clauses.Add(new Dictionary<string, object?>
        {
            ["terms"] = new Dictionary<string, object?> { [field] = present },
        });
    }

    private static Dictionary<string, object?> BuildDrillSidewaysAggregations(CatalogSearchFilter? filter)
    {
        var aggregations = new Dictionary<string, object?>
        {
            [CatalogFacetDimensions.Total] = new Dictionary<string, object?>
            {
                ["filter"] = FilterOrMatchAll(BuildFilterClauses(filter)),
            },
        };

        foreach (var (dimension, field) in Dimensions)
        {
            var others = BuildFilterClauses(Without(filter, dimension));

            aggregations[dimension] = new Dictionary<string, object?>
            {
                ["filter"] = FilterOrMatchAll(others),
                ["aggs"] = new Dictionary<string, object?>
                {
                    ["values"] = new Dictionary<string, object?>
                    {
                        ["terms"] = new Dictionary<string, object?>
                        {
                            ["field"] = field,
                            ["size"] = MaxFacetBuckets,
                        },
                    },
                },
            };
        }

        return aggregations;
    }

    private static Dictionary<string, object?> FilterOrMatchAll(List<object> clauses) =>
        clauses.Count == 0
            ? new Dictionary<string, object?> { ["match_all"] = new Dictionary<string, object?>() }
            : new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?> { ["filter"] = clauses.ToArray() },
            };

    internal static readonly (string Dimension, string Field)[] Dimensions =
    [
        (CatalogFacetDimensions.PublisherIdentifier, EsCatalogFields.PublisherIdentifierLabel),
        (CatalogFacetDimensions.Type, EsCatalogFields.Type),
        (CatalogFacetDimensions.Themes, EsCatalogFields.Themes),
        (CatalogFacetDimensions.AttributedAgentIdentifiers, EsCatalogFields.AttributedAgentIdentifiers),
        (CatalogFacetDimensions.AccessRights, EsCatalogFields.AccessRights),
        (CatalogFacetDimensions.Formats, EsCatalogFields.Formats),
        (CatalogFacetDimensions.Licenses, EsCatalogFields.Licenses),
        (CatalogFacetDimensions.BusinessEvents, EsCatalogFields.BusinessEvents),
        (CatalogFacetDimensions.LifeEvents, EsCatalogFields.LifeEvents),
        (CatalogFacetDimensions.ConceptType, EsCatalogFields.ConceptType),
        (CatalogFacetDimensions.PublicationLevel, EsCatalogFields.PublicationLevel),
        (CatalogFacetDimensions.PublicationLevelProposal, EsCatalogFields.PublicationLevelProposal),
        (CatalogFacetDimensions.RegistrationStatus, EsCatalogFields.RegistrationStatus),
        (CatalogFacetDimensions.RegistrationStatusProposal, EsCatalogFields.RegistrationStatusProposal),
        (CatalogFacetDimensions.HasStructure, EsCatalogFields.HasStructure),
        (CatalogFacetDimensions.Keywords, EsCatalogFields.KeywordFilter),
    ];

    private static CatalogSearchFilter? Without(CatalogSearchFilter? filter, string dimension)
    {
        if (filter is null)
        {
            return null;
        }

        return dimension switch
        {
            CatalogFacetDimensions.PublisherIdentifier => filter with { PublisherIdentifiers = [] },
            CatalogFacetDimensions.Type => filter with { Types = [] },
            CatalogFacetDimensions.Themes => filter with { Themes = [] },
            CatalogFacetDimensions.AttributedAgentIdentifiers => filter with { AttributedAgentIdentifiers = [] },
            CatalogFacetDimensions.AccessRights => filter with { AccessRights = [] },
            CatalogFacetDimensions.Formats => filter with { Formats = [] },
            CatalogFacetDimensions.Licenses => filter with { Licenses = [] },
            CatalogFacetDimensions.BusinessEvents => filter with { BusinessEvents = [] },
            CatalogFacetDimensions.LifeEvents => filter with { LifeEvents = [] },
            CatalogFacetDimensions.ConceptType => filter with { ConceptTypes = [] },
            CatalogFacetDimensions.PublicationLevel => filter with { PublicationLevels = [] },
            CatalogFacetDimensions.PublicationLevelProposal => filter with { PublicationLevelProposals = [] },
            CatalogFacetDimensions.RegistrationStatus => filter with { RegistrationStatuses = [] },
            CatalogFacetDimensions.RegistrationStatusProposal => filter with { RegistrationStatusProposals = [] },
            CatalogFacetDimensions.HasStructure => filter with { Structure = null },
            CatalogFacetDimensions.Keywords => filter with { Keywords = [] },
            _ => throw new NotSupportedException($"Unknown facet dimension '{dimension}'."),
        };
    }
}
