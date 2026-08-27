using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Vocabularies;

/// <summary>
/// Read-only access to the controlled vocabularies.
/// <para>
/// Deliberately the read half only. The full <c>IVocabulariesService</c> in <c>Bfs.Iop.Core</c> also
/// creates, updates and deletes vocabulary configurations, and those operations are authorization-
/// checked — which is why it derives from Core's authorized service base. Search needs none of that:
/// it turns codes stored in the index back into labels, per request, with no user in scope.
/// Extracting the read half is what lets the IndexSearch service resolve labels without referencing
/// the business layer.
/// </para>
/// <para>
/// <c>IVocabulariesService</c> extends this interface, so a single registration satisfies both and
/// every existing Core caller is unaffected.
/// </para>
/// </summary>
public interface IVocabularyReader
{
    /// <summary>
    /// Loads a vocabulary by its strongly-typed identity. Returns null when the vocabulary is not
    /// configured, rather than throwing — a missing vocabulary means unlabelled facets, not a failed
    /// request.
    /// </summary>
    Task<T?> TryGetVocabulary<T>(CancellationToken cancellationToken = default) where T : IdentifiedVocabularyBase, new();

    /// <summary>Loads a vocabulary by its identifier string. Null when not configured.</summary>
    Task<VocabularyModel?> TryGetVocabulary(string vocabularyIdentifier, CancellationToken cancellationToken);

    /// <summary>
    /// Warms the cache for every configured vocabulary in one pass, so a batch that resolves many
    /// codes does not issue a query per vocabulary.
    /// </summary>
    Task BuildAllVocabulariesInCache(CancellationToken cancellationToken = default);
}
