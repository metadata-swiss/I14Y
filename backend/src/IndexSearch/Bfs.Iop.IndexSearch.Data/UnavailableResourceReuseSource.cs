using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Data;

internal sealed class UnavailableResourceReuseSource : IResourceReuseSource
{
    public bool IsConfigured => false;

    private readonly ILogger<UnavailableResourceReuseSource> _logger;

    public UnavailableResourceReuseSource(ILogger<UnavailableResourceReuseSource> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyDictionary<Guid, int>?> GetReuseCountsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(
            "No IDatasetModelProcessService is registered, so the datasets conforming to a concept "
            + "cannot be counted. Reuse counts keep whatever values they already had, and search "
            + "orders equally relevant results the way it did before reuse was indexed. Configure "
            + "the triple store or the object store to fix this.");

        return Task.FromResult<IReadOnlyDictionary<Guid, int>?>(null);
    }
}
