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

        return vocabulariesService.GetThemes().FirstOrDefault(x => x.Code == input.Code)?.Uri
            ?? throw new InvalidOperationException($"The theme code '{input.Code}' does not resolve to any entry in a registered theme taxonomy.");
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
