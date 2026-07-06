using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Services.Extensions;

internal static class VocabulariesServiceExtensions
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
}
