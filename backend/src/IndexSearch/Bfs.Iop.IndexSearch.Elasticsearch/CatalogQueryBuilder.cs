using System.Text.RegularExpressions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogQueryBuilder
{
    private const int MaxFacetBuckets = 1000;

    // Elasticsearch refuses from + size beyond index.max_result_window (10,000 by default). Callers
    // conventionally ask for "everything" with a huge page size, which would be an error rather than
    // a full result set, so the window is clamped here instead.
    internal const int MaxResultWindow = 10_000;

    private static readonly Regex _emailQuery = new(
        @"^[^\s@""]+@[^\s@""]+\.[^\s@""]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static Dictionary<string, object?> BuildSearchBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        int from,
        int size) => new()
    {
        ["from"] = Math.Clamp(from, 0, MaxResultWindow),
        ["size"] = Math.Clamp(size, 0, MaxResultWindow - Math.Clamp(from, 0, MaxResultWindow)),
        ["query"] = WithRegistrationStatusBoost(BuildQuery(queryString, languages, filter, caller)),
    };


    private static Dictionary<string, object?> WithRegistrationStatusBoost(Dictionary<string, object?> query) => new()
    {
        ["function_score"] = new Dictionary<string, object?>
        {
            ["query"] = query,
            ["field_value_factor"] = new Dictionary<string, object?>
            {
                ["field"] = EsCatalogFields.RegistrationStatusWeight,
                ["factor"] = 0.01,
                ["missing"] = 100,
            },
            ["boost_mode"] = "multiply",
        },
    };

    public static Dictionary<string, object?> BuildCountBody(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller) => new()
    {
        ["size"] = 0,
        ["query"] = BuildQuery(queryString, languages, filter: null, caller),
        ["aggs"] = BuildDrillSidewaysAggregations(filter),
    };

    private static Dictionary<string, object?> BuildQuery(
        string? queryString,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller)
    {
        var must = new List<object>();
        var filters = new List<object>();

        if (!string.IsNullOrWhiteSpace(queryString))
        {
            must.Add(BuildFreeText(queryString, languages));
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
        IndexBusinessRole.SwissDataSteward or IndexBusinessRole.InteroperabilityService => null,
        IndexBusinessRole.StewardshipOrganisationViewer
            or IndexBusinessRole.LocalDataSteward
            or IndexBusinessRole.Submitter => AgencyScoped(caller.Agencies),
        _ => PublicOnly(),
    };

    private static Dictionary<string, object?> PublicOnly() => new()
    {
        ["term"] = new Dictionary<string, object?>
        {
            [EsCatalogFields.PublicationLevel] = IndexPublicationLevel.Public.ToString(),
        },
    };

    private static Dictionary<string, object?> AgencyScoped(IReadOnlyList<string> agencies)
    {
        var should = new List<object> { PublicOnly() };

        if (agencies.Count > 0)
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
                                [EsCatalogFields.PublicationLevel] = IndexPublicationLevel.Internal.ToString(),
                            },
                        },
                        new Dictionary<string, object?>
                        {
                            ["terms"] = new Dictionary<string, object?>
                            {
                                [EsCatalogFields.PublisherIdentifier] =
                                    agencies.Select(x => x.ToLowerInvariant()).ToArray(),
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

    private static Dictionary<string, object?> BuildFreeText(string queryString, IReadOnlyList<string> languages)
    {
        var trimmed = queryString.Trim().Trim('"').Trim();

        if (_emailQuery.IsMatch(trimmed))
        {
            return AnyOf([.. EsCatalogFields.EmailFields.Select(x => ExactTerm(x, trimmed))]);
        }

        var fields = new List<string>();

        foreach (var field in EsCatalogFields.MultiLanguageFields)
        {
            foreach (var language in languages)
            {
                fields.Add($"{field}.{language}");
                fields.Add($"{field}.{language}.ngram");
            }
        }

        fields.AddRange(EsCatalogFields.SearchableKeywordFields
            .Select(EsCatalogFields.TextOf));

        var should = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["multi_match"] = new Dictionary<string, object?>
                {
                    ["query"] = queryString,
                    ["fields"] = fields.ToArray(),
                    ["type"] = "best_fields",
                },
            },
        };


        should.Add(ExactTerm(EsCatalogFields.Id, queryString));

        foreach (var emailField in EsCatalogFields.EmailFields)
        {
            should.Add(ExactTerm(emailField, queryString));
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
        AddTerms(clauses, EsCatalogFields.AccessRights, filter.AccessRights);
        AddTerms(clauses, EsCatalogFields.Formats, filter.Formats);
        AddTerms(clauses, EsCatalogFields.BusinessEvents, filter.BusinessEvents);
        AddTerms(clauses, EsCatalogFields.LifeEvents, filter.LifeEvents);
        AddTerms(clauses, EsCatalogFields.Type, filter.Types.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.ConceptType, filter.ConceptTypes.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.PublicationLevel, filter.PublicationLevels.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.PublicationLevelProposal, filter.PublicationLevelProposals.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.RegistrationStatus, filter.RegistrationStatuses.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.RegistrationStatusProposal, filter.RegistrationStatusProposals.Select(x => x.ToString()));
        AddTerms(clauses, EsCatalogFields.PublisherIdentifier,
            filter.PublisherIdentifiers.Select(x => x.ToLowerInvariant()));

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
        (CatalogFacetDimensions.AccessRights, EsCatalogFields.AccessRights),
        (CatalogFacetDimensions.Formats, EsCatalogFields.Formats),
        (CatalogFacetDimensions.BusinessEvents, EsCatalogFields.BusinessEvents),
        (CatalogFacetDimensions.LifeEvents, EsCatalogFields.LifeEvents),
        (CatalogFacetDimensions.ConceptType, EsCatalogFields.ConceptType),
        (CatalogFacetDimensions.PublicationLevel, EsCatalogFields.PublicationLevel),
        (CatalogFacetDimensions.PublicationLevelProposal, EsCatalogFields.PublicationLevelProposal),
        (CatalogFacetDimensions.RegistrationStatus, EsCatalogFields.RegistrationStatus),
        (CatalogFacetDimensions.RegistrationStatusProposal, EsCatalogFields.RegistrationStatusProposal),
        (CatalogFacetDimensions.HasStructure, EsCatalogFields.HasStructure),
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
            CatalogFacetDimensions.AccessRights => filter with { AccessRights = [] },
            CatalogFacetDimensions.Formats => filter with { Formats = [] },
            CatalogFacetDimensions.BusinessEvents => filter with { BusinessEvents = [] },
            CatalogFacetDimensions.LifeEvents => filter with { LifeEvents = [] },
            CatalogFacetDimensions.ConceptType => filter with { ConceptTypes = [] },
            CatalogFacetDimensions.PublicationLevel => filter with { PublicationLevels = [] },
            CatalogFacetDimensions.PublicationLevelProposal => filter with { PublicationLevelProposals = [] },
            CatalogFacetDimensions.RegistrationStatus => filter with { RegistrationStatuses = [] },
            CatalogFacetDimensions.RegistrationStatusProposal => filter with { RegistrationStatusProposals = [] },
            CatalogFacetDimensions.HasStructure => filter with { Structure = null },
            _ => throw new NotSupportedException($"Unknown facet dimension '{dimension}'."),
        };
    }
}
