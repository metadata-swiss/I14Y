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

    public static IReadOnlyList<VocabularyEntryModel> GetStandardThemes(this IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var registry = vocabulariesService.GetExistingOrEmptyVocabulary<ThemeTaxonomiesVocabulary>();

        return registry.Entries
            .SelectMany(taxonomy =>
                vocabulariesService.TryGetVocabulary(taxonomy.Code, CancellationToken.None).GetAwaiter().GetResult()?.Entries ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Uri))
            // A URI is one theme, under the taxonomy the registry lists first.
            .DistinctBy(x => x.Uri, StringComparer.Ordinal)
            .ToList();
    }

    // Standard first, so a shared URI keeps its registry taxonomy.
    public static IReadOnlyList<VocabularyEntryModel> GetAllThemes(this IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var byUri = new Dictionary<string, VocabularyEntryModel>(StringComparer.Ordinal);

        foreach (var theme in vocabulariesService.GetStandardThemes())
        {
            byUri.TryAdd(theme.Uri!, theme);
        }

        var configs = vocabulariesService.GetVocabularyConfigs(CancellationToken.None).GetAwaiter().GetResult();

        foreach (var config in configs)
        {
            var vocabulary = vocabulariesService
                .TryGetVocabulary(config.VocabularyIdentifier, CancellationToken.None)
                .GetAwaiter().GetResult();

            foreach (var entry in vocabulary?.Entries ?? [])
            {
                if (!string.IsNullOrWhiteSpace(entry.Uri))
                {
                    byUri.TryAdd(entry.Uri, entry);
                }
            }
        }

        return [.. byUri.Values];
    }

    // A named vocabulary is taken as given: nothing distinguishes a theme vocabulary from another.
    public static IReadOnlyList<VocabularyEntryModel> GetThemesFor(this IVocabulariesService vocabulariesService, ThemeInputModel input)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        if (string.IsNullOrWhiteSpace(input.VocabularyIdentifier))
        {
            return vocabulariesService.GetStandardThemes();
        }

        var vocabulary = vocabulariesService
            .TryGetVocabulary(input.VocabularyIdentifier, CancellationToken.None)
            .GetAwaiter().GetResult();

        return [.. (vocabulary?.Entries ?? []).Where(x => !string.IsNullOrWhiteSpace(x.Uri))];
    }

    public static string ResolveThemeInputToUri(this IVocabulariesService vocabulariesService, ThemeInputModel input)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        if (!string.IsNullOrWhiteSpace(input.Uri))
        {
            return input.Uri;
        }

        // ThemeInputModelValidator rejects an ambiguous code before this point.
        var uris = vocabulariesService.GetThemesFor(input)
            .Where(x => x.Code == input.Code)
            .Select(x => x.Uri!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var where = string.IsNullOrWhiteSpace(input.VocabularyIdentifier)
            ? "any registered theme taxonomy"
            : $"'{input.VocabularyIdentifier}'";

        return uris.Count switch
        {
            1 => uris[0],
            0 => throw new InvalidOperationException($"The theme code '{input.Code}' does not resolve to any entry in {where}."),
            _ => throw new InvalidOperationException($"The theme code '{input.Code}' exists in several registered theme taxonomies; the URI or the vocabulary identifier is required to tell them apart.")
        };
    }

    public static IReadOnlyList<VocabularyEntryModel> ResolveThemeValues(this IVocabulariesService vocabulariesService, IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        var themesByUri = vocabulariesService.GetAllThemes()
            .ToDictionary(x => x.Uri!, StringComparer.Ordinal);

        // A stored URI outside the registered taxonomies means something was missed; a fabricated entry
        // would be indistinguishable from a real theme in the search index and the RDF export.
        return values
            .Select(value => themesByUri.TryGetValue(value, out var theme)
                ? theme
                : throw new InvalidOperationException($"The stored theme '{value}' belongs to no vocabulary defined on I14Y."))
            .ToList();
    }
}
