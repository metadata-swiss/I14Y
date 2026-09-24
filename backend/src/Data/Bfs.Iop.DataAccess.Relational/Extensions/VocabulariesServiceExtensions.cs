using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Extensions;

public static class VocabulariesServiceExtensions
{
    public static T GetExistingOrEmptyVocabulary<T>(this IVocabulariesService vocabulariesService) where T : IdentifiedVocabularyBase, new()
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var vocabulary = vocabulariesService.TryGetVocabulary<T>(CancellationToken.None).GetAwaiter().GetResult();

        return vocabulary ?? new();
    }

    public static VocabularyModel GetExistingOrEmptyVocabulary(
        this IVocabulariesService vocabulariesService, 
        string vocabularyIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vocabularyIdentifier, nameof(vocabularyIdentifier));

        var vocabulary = vocabulariesService.TryGetVocabulary(vocabularyIdentifier, CancellationToken.None).GetAwaiter().GetResult();

        return vocabulary ?? new() { Identifier = vocabularyIdentifier };
    }

    // A taxonomy listed in the registry but not resolving to a registered vocabulary is skipped,
    // rather than failing every theme read.
    public static IReadOnlyList<VocabularyEntryModel> GetThemes(this IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var registry = vocabulariesService.GetExistingOrEmptyVocabulary<ThemeTaxonomiesVocabulary>();

        return registry.Entries
            .SelectMany(taxonomy =>
                vocabulariesService.TryGetVocabulary(taxonomy.Code, CancellationToken.None).GetAwaiter().GetResult()?.Entries
                    ?? [])
            .Where(x => x.Uri is not null)
            .ToList();
    }

    public static string ResolveThemeInputToUri(this IVocabulariesService vocabulariesService, ThemeInputModel input)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        if (!string.IsNullOrWhiteSpace(input.Uri))
        {
            return input.Uri;
        }

        // Codes are only unique within a taxonomy, so a code matching several of them is refused rather
        // than resolved to whichever comes first. ThemeInputModelValidator rejects it before this point.
        var uris = vocabulariesService.GetThemes()
            .Where(x => x.Code == input.Code)
            .Select(x => x.Uri!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return uris.Count switch
        {
            1 => uris[0],
            0 => throw new InvalidOperationException($"The theme code '{input.Code}' does not resolve to any entry in a registered theme taxonomy."),
            _ => throw new InvalidOperationException($"The theme code '{input.Code}' exists in several registered theme taxonomies; the URI is required to tell them apart.")
        };
    }

    public static IReadOnlyList<VocabularyEntryModel> ResolveThemeValues(this IVocabulariesService vocabulariesService, IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        var themesByUri = vocabulariesService.GetThemes()
            .GroupBy(x => x.Uri, StringComparer.Ordinal)
            .ToDictionary(x => x.Key!, x => x.First(), StringComparer.Ordinal);

        return values
            .Select(value =>
                themesByUri.TryGetValue(value, out var theme) ? theme
                : new VocabularyEntryModel { Code = value, Uri = value })
            .ToList();
    }
}
