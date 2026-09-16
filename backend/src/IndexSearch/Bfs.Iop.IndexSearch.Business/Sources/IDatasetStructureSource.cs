namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface IDatasetStructureSource
{
    bool IsConfigured { get; }

    Task<IReadOnlySet<Guid>?> GetIdsWithStructuresAsync(CancellationToken cancellationToken = default);
}
