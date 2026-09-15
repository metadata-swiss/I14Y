using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Data;

internal sealed class UnavailableDatasetStructureSource : IDatasetStructureSource
{
    private readonly ILogger<UnavailableDatasetStructureSource> _logger;

    public UnavailableDatasetStructureSource(ILogger<UnavailableDatasetStructureSource> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlySet<Guid>?> GetIdsWithStructuresAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogError(
            "No IDatasetModelProcessService is registered, so dataset structures cannot be read. On a "
            + "reindex the structure flag keeps whatever value it already had and the Structures facet "
            + "goes stale; on a reset it is never written at all and the facet comes back empty. "
            + "Configure the triple store or the object store to fix this.");

        return Task.FromResult<IReadOnlySet<Guid>?>(null);
    }
}
