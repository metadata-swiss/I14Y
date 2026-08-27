using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

namespace Bfs.Iop.Search.Elasticsearch.CodeList;

/// <summary>
/// Builds the Elasticsearch query for code-list-entry search. The ranking is:
/// Code ^20 (prefix), Name-ngram ^16 (≥60% soft match), Description ^12 (fuzzy), annotation fields ^8
/// (nested). AND across terms, OR across fields. Always scoped by conceptId; annotation filters are
/// applied as nested queries.
/// </summary>
internal static class CodeListQueryBuilder
{
    private const double CodeBoost = 20.0;
    private const double NameNgramBoost = 16.0;
    private const double DescriptionBoost = 12.0;
    private const double AnnotationBoost = 8.0;

    public static Dictionary<string, object?> BuildSearchBody(
        Guid conceptId,
        string language,
        string? query,
        IReadOnlyList<FilterInputModel> parsedFilters,
        FilterConfigurationModel filterConfiguration,
        int from,
        int size)
    {
        var filters = new List<object>
        {
            Term(EsCodeListFields.ConceptId, conceptId.ToString()),
        };

        AddAnnotationFilters(filters, parsedFilters, filterConfiguration);

        return new Dictionary<string, object?>
        {
            ["from"] = from,
            ["size"] = size,
            ["track_total_hits"] = true,
            ["_source"] = new[] { EsCodeListFields.Id },
            ["query"] = new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?>
                {
                    ["must"] = new[] { BuildTextQuery(query, language) },
                    ["filter"] = filters,
                },
            },
        };
    }

    private static object BuildTextQuery(string? query, string language)
    {
        var q = string.IsNullOrWhiteSpace(query) ? "*" : query.Trim().ToLowerInvariant();
        if (q == "*")
        {
            return MatchAll();
        }

        var terms = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2)
            .Distinct()
            .ToArray();

        if (terms.Length == 0)
        {
            return MatchAll();
        }

        // AND across terms; each term OR across the boosted fields.
        var must = terms.Select(term => (object)new Dictionary<string, object?>
        {
            ["bool"] = new Dictionary<string, object?>
            {
                ["should"] = BuildPerTermFieldClauses(term, language),
                ["minimum_should_match"] = 1,
            },
        }).ToArray();

        return new Dictionary<string, object?> { ["bool"] = new Dictionary<string, object?> { ["must"] = must } };
    }

    private static object[] BuildPerTermFieldClauses(string term, string language)
    {
        return
        [
            // Code — prefix, highest boost.
            new Dictionary<string, object?>
            {
                ["prefix"] = new Dictionary<string, object?>
                {
                    [EsCodeListFields.Code] = new Dictionary<string, object?> { ["value"] = term, ["boost"] = CodeBoost },
                },
            },
            // Name (ngram) — soft match on ≥60% of grams.
            new Dictionary<string, object?>
            {
                ["match"] = new Dictionary<string, object?>
                {
                    [$"{EsCodeListFields.Name}.{language}.ngram"] = new Dictionary<string, object?>
                    {
                        ["query"] = term,
                        ["minimum_should_match"] = "60%",
                        ["boost"] = NameNgramBoost,
                    },
                },
            },
            // Description — fuzzy.
            new Dictionary<string, object?>
            {
                ["match"] = new Dictionary<string, object?>
                {
                    [$"{EsCodeListFields.Description}.{language}"] = new Dictionary<string, object?>
                    {
                        ["query"] = term,
                        ["fuzziness"] = "AUTO",
                        ["boost"] = DescriptionBoost,
                    },
                },
            },
            // Annotations — nested, fuzzy across annotation fields.
            new Dictionary<string, object?>
            {
                ["nested"] = new Dictionary<string, object?>
                {
                    ["path"] = EsCodeListFields.Annotations,
                    ["score_mode"] = "max",
                    ["boost"] = AnnotationBoost,
                    ["query"] = new Dictionary<string, object?>
                    {
                        ["multi_match"] = new Dictionary<string, object?>
                        {
                            ["query"] = term,
                            ["fuzziness"] = "AUTO",
                            ["fields"] = new[]
                            {
                                EsCodeListFields.Nested(EsCodeListFields.AnnotationType),
                                EsCodeListFields.Nested(EsCodeListFields.AnnotationIdentifier),
                                EsCodeListFields.Nested(EsCodeListFields.AnnotationTitle),
                                EsCodeListFields.Nested(EsCodeListFields.AnnotationUri),
                                $"{EsCodeListFields.Annotations}.{EsCodeListFields.AnnotationText}.{language}",
                            },
                        },
                    },
                },
            },
        ];
    }

    // Resolve the input filters against the concept's filter
    // configuration and turn each selected field-value into a nested annotation query.
    private static void AddAnnotationFilters(
        List<object> filters,
        IReadOnlyList<FilterInputModel> parsedFilters,
        FilterConfigurationModel filterConfiguration)
    {
        if (parsedFilters.Count == 0)
        {
            return;
        }

        var andQueries = new List<object>();
        var orQueries = new List<object>();

        foreach (var parsedFilter in parsedFilters)
        {
            var definition = filterConfiguration.Filters
                .SingleOrDefault(f => f.FilterIdentifier.Equals(parsedFilter.FilterIdentifier, StringComparison.InvariantCultureIgnoreCase))
                ?? throw new InvalidOperationException($"Filter with identifier {parsedFilter.FilterIdentifier} does not exist.");

            var selectedValues = definition.Values
                .Where(v => parsedFilter.Values.Select(x => x.ToLowerInvariant()).Contains(v.FilterValueIdentifier.ToLowerInvariant()));

            foreach (var selected in selectedValues)
            {
                foreach (var fieldValue in selected.FieldValues)
                {
                    var nested = BuildAnnotationFieldValueQuery(fieldValue);
                    if (selected.OccurType == OccurType.Must)
                    {
                        andQueries.Add(nested);
                    }
                    else
                    {
                        orQueries.Add(nested);
                    }
                }
            }
        }

        filters.AddRange(andQueries);

        if (orQueries.Count > 0)
        {
            filters.Add(new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?> { ["should"] = orQueries, ["minimum_should_match"] = 1 },
            });
        }
    }

    private static object BuildAnnotationFieldValueQuery(FilterDefinitionValueModel fieldValue)
    {
        var must = new List<object>
        {
            TermRaw(EsCodeListFields.AnnotationType, fieldValue.FieldName.ToLowerInvariant()),
        };

        if (!string.IsNullOrWhiteSpace(fieldValue.FieldValue))
        {
            var sub = fieldValue.FieldProperty?.ToLowerInvariant() switch
            {
                "title" => EsCodeListFields.AnnotationTitle,
                "identifier" => EsCodeListFields.AnnotationIdentifier,
                _ => EsCodeListFields.AnnotationType,
            };

            must.Add(TermRaw(sub, fieldValue.FieldValue.ToLowerInvariant()));
        }
        else if (fieldValue.FieldValueMultilanguage is not null)
        {
            var should = fieldValue.FieldValueMultilanguage.ToDictionary()
                .Select(kv => (object)TermRaw($"{EsCodeListFields.AnnotationText}.{kv.Key}", kv.Value.ToLowerInvariant()))
                .ToList();

            must.Add(new Dictionary<string, object?>
            {
                ["bool"] = new Dictionary<string, object?> { ["should"] = should, ["minimum_should_match"] = 1 },
            });
        }

        return new Dictionary<string, object?>
        {
            ["nested"] = new Dictionary<string, object?>
            {
                ["path"] = EsCodeListFields.Annotations,
                ["query"] = new Dictionary<string, object?> { ["bool"] = new Dictionary<string, object?> { ["must"] = must } },
            },
        };
    }

    private static Dictionary<string, object?> Term(string field, string value) => new()
    {
        ["term"] = new Dictionary<string, object?> { [field] = value },
    };

    // Term on the `.raw` keyword sub-field of a nested annotation field.
    private static Dictionary<string, object?> TermRaw(string annotationSubField, string value) => new()
    {
        ["term"] = new Dictionary<string, object?> { [$"{EsCodeListFields.Annotations}.{annotationSubField}.raw"] = value },
    };

    private static Dictionary<string, object?> MatchAll() => new()
    {
        ["match_all"] = new Dictionary<string, object?>(),
    };
}
