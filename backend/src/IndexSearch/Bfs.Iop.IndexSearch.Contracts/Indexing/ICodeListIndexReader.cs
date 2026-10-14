namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public interface ICodeListIndexReader
{
    Task<IReadOnlyCollection<Guid>> ReadEntryIdsAsync(
        Guid conceptId,
        CancellationToken cancellationToken = default);
}
