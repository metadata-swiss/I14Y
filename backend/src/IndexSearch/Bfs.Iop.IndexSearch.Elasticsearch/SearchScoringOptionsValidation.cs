using System.Text.RegularExpressions;
using Bfs.Iop.DataAccess.Abstractions;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class SearchScoringOptionsValidation : IValidateOptions<SearchScoringOptions>
{
    private static readonly Regex _minimumShouldMatch = new(
        @"^-?\d+%?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> _searchedFields = new(
        [.. EsCatalogFields.MultiLanguageFields, .. EsCatalogFields.SearchableKeywordFields],
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> _statuses = new(
        Enum.GetNames<RegistrationStatus>(),
        StringComparer.OrdinalIgnoreCase);

    public ValidateOptionsResult Validate(string? name, SearchScoringOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        ValidateCatalog(failures, options.Catalog);
        ValidateCodeList(failures, options.CodeList);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateCatalog(List<string> failures, CatalogScoring catalog)
    {
        foreach (var (field, weight) in catalog.FieldWeights)
        {
            // A misspelt field would otherwise be ignored and the real one left at 1.
            if (!_searchedFields.Contains(field))
            {
                failures.Add(
                    $"'{Key("Catalog:FieldWeights")}' names '{field}', which the catalogue search does not "
                    + $"search. Searched fields: {string.Join(", ", _searchedFields)}.");
            }

            RequirePositive(failures, $"Catalog:FieldWeights:{field}", weight);
        }

        foreach (var status in catalog.RegistrationStatusWeights.Keys.Where(x => !_statuses.Contains(x)))
        {
            failures.Add(
                $"'{Key("Catalog:RegistrationStatusWeights")}' names '{status}', which is not a registration "
                + $"status. Statuses: {string.Join(", ", _statuses)}.");
        }

        RequirePositive(failures, "Catalog:PartialMatchBoost", catalog.PartialMatchBoost);
        RequireMinimumShouldMatch(failures, "Catalog:PartialMatchMinimumShouldMatch", catalog.PartialMatchMinimumShouldMatch);
    }

    private static void ValidateCodeList(List<string> failures, CodeListScoring codeList)
    {
        RequirePositive(failures, "CodeList:CodeBoost", codeList.CodeBoost);
        RequirePositive(failures, "CodeList:NameBoost", codeList.NameBoost);
        RequirePositive(failures, "CodeList:DescriptionBoost", codeList.DescriptionBoost);
        RequirePositive(failures, "CodeList:AnnotationBoost", codeList.AnnotationBoost);
        RequireMinimumShouldMatch(failures, "CodeList:NgramMinimumShouldMatch", codeList.NgramMinimumShouldMatch);

        if (codeList.SingleEditMaxTermLength < 0)
        {
            failures.Add($"'{Key("CodeList:SingleEditMaxTermLength")}' cannot be negative.");
        }
    }

    private static void RequirePositive(List<string> failures, string key, double value)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            failures.Add($"'{Key(key)}' has to be a number above 0, not '{value}'.");
        }
    }

    private static void RequireMinimumShouldMatch(List<string> failures, string key, string? value)
    {
        if (value is null || !_minimumShouldMatch.IsMatch(value))
        {
            failures.Add($"'{Key(key)}' has to be a whole number or a percentage such as '75%', not '{value}'.");
        }
    }

    private static string Key(string key) => $"{SearchScoringOptions.SectionName}:{key}";
}
