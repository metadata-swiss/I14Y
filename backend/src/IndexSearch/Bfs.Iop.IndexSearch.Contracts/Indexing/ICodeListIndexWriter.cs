namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public interface ICodeListIndexWriter
{
    Task<int> WriteAsync(
        IReadOnlyCollection<CodeListIndexDocument> documents,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}