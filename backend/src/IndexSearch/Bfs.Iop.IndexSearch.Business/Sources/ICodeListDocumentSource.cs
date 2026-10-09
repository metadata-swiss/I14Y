using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface ICodeListDocumentSource
{
    IAsyncEnumerable<IReadOnlyList<CodeListIndexDocument>> ReadAllAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Every entry of one concept, with ancestor codes resolved within that concept.
    /// </summary>
    Task<IReadOnlyList<CodeListIndexDocument>> ReadConceptAsync(
        Guid conceptId,
        CancellationToken cancellationToken = default);
}