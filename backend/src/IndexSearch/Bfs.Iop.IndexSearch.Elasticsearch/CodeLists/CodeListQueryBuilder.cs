using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal static class CodeListQueryBuilder
{
    internal const int MaxResultWindow = 100_000;
    private const int MinimumTermLength = 2;

    public static Dictionary<string, object?> BuildSearchBody(
        Guid conceptId,
        string? queryString,
        string language,
        CodeListSearchFilter? filter,
        CodeListScoring scoring,
        int from,
        int size) => new()
        {
            ["from"] = from,
            ["size"] = size,
            ["track_total_hits"] = true,
            ["query"] = BuildQuery(conceptId, queryString, language, filter, scoring),
        };


    /// <summary>
    ///     A page of a point-in-time walk. Sorted by <c>_shard_doc</c>, which is the cheapest total
    ///     order Elasticsearch offers and exists only inside a point in time. Relevance order is
    ///     meaningless for an export and is not a tiebreaker, so a walk sorted by score could repeat
    ///     or skip entries.
    /// </summary>
    public static Dictionary<string, object?> BuildPointInTimeBody(
        Guid conceptId,
        string? queryString,
        string language,
        CodeListSearchFilter? filter,
        CodeListScoring scoring,
        string pointInTimeId,
        TimeSpan keepAlive,
        int size,
        IReadOnlyList<object>? searchAfter)
    {
        var body = new Dictionary<string, object?>
        {
            ["size"] = size,
            ["track_total_hits"] = false,
            ["query"] = BuildQuery(conceptId, queryString, language, filter, scoring),
            ["sort"] = new object[]
            {
                new Dictionary<string, object?> { ["_score"] = "desc" },
                new Dictionary<string, object?> { ["_shard_doc"] = "asc" },
            },
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
    private static Dictionary<string, object?> BuildQuery(
        Guid conceptId,
        string? queryString,
        string language,
        CodeListSearchFilter? filter,
        CodeListScoring scoring)
    {
        var boolQuery = new Dictionary<string, object?>();

        var freeText = BuildFreeText(queryString, language, scoring);

        if (freeText is not null)
        {
            boolQuery["must"] = new object[] { freeText };
        }

        var filters = new List<object>
        {
            Term(EsCodeListFields.ConceptId, conceptId.ToString()),
        };

        filters.AddRange(BuildFilterClauses(filter));

        boolQuery["filter"] = filters.ToArray();

        return new Dictionary<string, object?> { ["bool"] = boolQuery };
    }

    private static Dictionary<string, object?>? BuildFreeText(string? queryString, string language, CodeListScoring scoring)
    {
        var trimmed = queryString?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed == "*")
        {
            return null;
        }

        var terms = trimmed
            .ToLowerInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        if (terms.Length == 0)
        {
            return null;
        }

        return new Dictionary<string, object?>
        {
            ["bool"] = new Dictionary<string, object?>
            {
                ["must"] = terms.Select(x => AnyFieldMatches(x, language, scoring)).ToArray(),
            },
        };
    }

    private static Dictionary<string, object?> AnyFieldMatches(string term, string language, CodeListScoring scoring)
    {
        var should = new List<object>
        {
            term.Length > MinimumTermLength
                ? Prefix(EsCodeListFields.Code, term, scoring.CodeBoost)
                : Term(EsCodeListFields.Code, term, scoring.CodeBoost),

            Match($"{EsCodeListFields.Name}.{language}", term, scoring.NameBoost, fuzziness: FuzzinessFor(term, scoring)),

            Match($"{EsCodeListFields.Description}.{language}", term, scoring.DescriptionBoost, fuzziness: FuzzinessFor(term, scoring)),

            AnnotationMatches(term, language, scoring),
        };

        if (term.Length >= MinimumTermLength)
        {
            should.Add(Match(
                $"{EsCodeListFields.Name}.{language}.ngram",
                term,
                scoring.NameBoost,
                minimumShouldMatch: scoring.NgramMinimumShouldMatch));
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

    private static Dictionary<string, object?> AnnotationMatches(string term, string language, CodeListScoring scoring)
    {
        var a = EsCodeListFields.Annotations;

        var should = new object[]
        {
            term.Length > MinimumTermLength
                ? Prefix($"{a}.{EsCodeListFields.Annotation.Identifier}", term)
                : Term($"{a}.{EsCodeListFields.Annotation.Identifier}", term),

            term.Length > MinimumTermLength
                ? Prefix($"{a}.{EsCodeListFields.Annotation.Uri}", term)
                : Term($"{a}.{EsCodeListFields.Annotation.Uri}", term),

            Match($"{a}.{EsCodeListFields.Annotation.Type}.{EsAnalysis.TextSubField}", term, fuzziness: FuzzinessFor(term, scoring)),
            Match($"{a}.{EsCodeListFields.Annotation.Title}.{EsAnalysis.TextSubField}", term, fuzziness: FuzzinessFor(term, scoring)),
            Match($"{a}.{EsCodeListFields.Annotation.Text}.{language}", term, fuzziness: FuzzinessFor(term, scoring)),
        };

        return new Dictionary<string, object?>
        {
            ["nested"] = new Dictionary<string, object?>
            {
                ["path"] = a,
                ["boost"] = scoring.AnnotationBoost,
                ["query"] = new Dictionary<string, object?>
                {
                    ["bool"] = new Dictionary<string, object?>
                    {
                        ["should"] = should,
                        ["minimum_should_match"] = 1,
                    },
                },
            },
        };
    }

    private static int? FuzzinessFor(string term, CodeListScoring scoring) => term.Length < MinimumTermLength
        ? null
        : term.Length <= scoring.SingleEditMaxTermLength ? 1 : 2;

    private static List<object> BuildFilterClauses(CodeListSearchFilter? filter)
    {
        var clauses = new List<object>();

        if (filter is null || filter.IsEmpty)
        {
            return clauses;
        }

        clauses.AddRange(filter.All.Select(NestedCriterion));

        if (filter.Any.Count > 0)
        {
            clauses.Add(new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?>
                {
                    ["should"] = filter.Any.Select(NestedCriterion).ToArray(),
                    ["minimum_should_match"] = 1,
                },
            });
        }

        return clauses;
    }

    private static Dictionary<string, object?> NestedCriterion(CodeListAnnotationCriterion criterion)
    {
        var a = EsCodeListFields.Annotations;

        var must = new List<object>
        {
            Term($"{a}.{EsCodeListFields.Annotation.Type}", criterion.Type),
        };

        if (!string.IsNullOrWhiteSpace(criterion.Value) && criterion.Property is not null)
        {
            var field = criterion.Property switch
            {
                CodeListAnnotationProperty.Identifier => EsCodeListFields.Annotation.Identifier,
                CodeListAnnotationProperty.Title => EsCodeListFields.Annotation.Title,
                _ => EsCodeListFields.Annotation.Type,
            };

            must.Add(Term($"{a}.{field}", criterion.Value));
        }
        else if (criterion.Text is not null && !criterion.Text.IsContentNullOrWhiteSpace())
        {
            must.Add(new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?>
                {
                    ["should"] = criterion.Text.ToDictionary()
                        .Select(x => Term(
                            $"{a}.{EsCodeListFields.Annotation.Text}.{x.Key}.{EsAnalysis.RawSubField}",
                            x.Value))
                        .ToArray(),
                    ["minimum_should_match"] = 1,
                },
            });
        }

        return new Dictionary<string, object?>
        {
            ["nested"] = new Dictionary<string, object?>
            {
                ["path"] = a,
                ["query"] = new Dictionary<string, object?>
                {
                    ["bool"] = new Dictionary<string, object?> { ["must"] = must.ToArray() },
                },
            },
        };
    }

    private static Dictionary<string, object?> Term(string field, string value, double? boost = null)
    {
        var term = new Dictionary<string, object?> { ["value"] = value };

        if (boost.HasValue)
        {
            term["boost"] = boost.Value;
        }

        return new Dictionary<string, object?>
        {
            ["term"] = new Dictionary<string, object?> { [field] = term },
        };
    }

    private static Dictionary<string, object?> Prefix(string field, string value, double? boost = null)
    {
        var prefix = new Dictionary<string, object?> { ["value"] = value };

        if (boost.HasValue)
        {
            prefix["boost"] = boost.Value;
        }

        return new Dictionary<string, object?>
        {
            ["prefix"] = new Dictionary<string, object?> { [field] = prefix },
        };
    }

    private static Dictionary<string, object?> Match(
        string field,
        string query,
        double? boost = null,
        int? fuzziness = null,
        string? minimumShouldMatch = null)
    {
        var match = new Dictionary<string, object?> { ["query"] = query };

        if (boost.HasValue)
        {
            match["boost"] = boost.Value;
        }

        if (fuzziness.HasValue)
        {
            match["fuzziness"] = fuzziness.Value;
            match["prefix_length"] = 1;
        }

        if (minimumShouldMatch is not null)
        {
            match["minimum_should_match"] = minimumShouldMatch;
        }

        return new Dictionary<string, object?>
        {
            ["match"] = new Dictionary<string, object?> { [field] = match },
        };
    }
}
