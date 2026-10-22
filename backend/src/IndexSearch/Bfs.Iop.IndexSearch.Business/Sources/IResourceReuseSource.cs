namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface IResourceReuseSource
{
    bool IsConfigured { get; }

    Task<IReadOnlyDictionary<Guid, int>?> GetReuseCountsAsync(CancellationToken cancellationToken = default);
}
