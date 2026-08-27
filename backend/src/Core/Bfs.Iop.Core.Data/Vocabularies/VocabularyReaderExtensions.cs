using Bfs.Iop.Core.Abstractions.Models;

using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Vocabularies;

public static class VocabularyReaderExtensions
{
    public static T GetExistingOrEmptyVocabulary<T>(this IVocabularyReader vocabulariesService) where T : IdentifiedVocabularyBase, new()
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var vocabulary = vocabulariesService.TryGetVocabulary<T>(CancellationToken.None).GetAwaiter().GetResult();

        return vocabulary ?? new();
    }

    public static VocabularyModel GetExistingOrEmptyVocabulary(
        this IVocabularyReader vocabulariesService, 
        string vocabularyIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vocabularyIdentifier, nameof(vocabularyIdentifier));

        var vocabulary = vocabulariesService.TryGetVocabulary(vocabularyIdentifier, CancellationToken.None).GetAwaiter().GetResult();

        return vocabulary ?? new() { Identifier = vocabularyIdentifier };
    }
}
