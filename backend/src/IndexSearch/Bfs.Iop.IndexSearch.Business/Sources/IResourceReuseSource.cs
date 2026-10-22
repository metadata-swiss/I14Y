namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface IResourceReuseSource
{
    Task<IReadOnlyDictionary<Guid, int>?> GetReuseCountsAsync(CancellationToken cancellationToken = default);
}
