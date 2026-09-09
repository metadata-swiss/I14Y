namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface IDatasetStructureSource
{
    Task<IReadOnlySet<Guid>?> GetIdsWithStructuresAsync(CancellationToken cancellationToken = default);
}
