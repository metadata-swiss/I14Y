using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Data;

internal sealed class DatasetStructureSource : IDatasetStructureSource
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly ILogger<DatasetStructureSource> _logger;

    public DatasetStructureSource(
        IDatasetModelProcessService datasetModelProcessService,
        ILogger<DatasetStructureSource> logger)
    {
        _datasetModelProcessService = datasetModelProcessService;
        _logger = logger;
    }

    public async Task<IReadOnlySet<Guid>?> GetIdsWithStructuresAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<string> raw;

        try
        {
            raw = await _datasetModelProcessService.GetAllDatasetIdsWithStructures(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Null, not empty: an unreadable store must leave the indexed flag as it is rather than
            // clearing it from every dataset for the duration of the outage.
            _logger.LogError(ex, "The dataset structures could not be listed.");
            return null;
        }

        var ids = new HashSet<Guid>();

        foreach (var value in raw)
        {
            // The two implementations behind this interface return different shapes — the file-backed
            // one filenames ("{guid}.ttl"), the triple store bare ids — and a caller cannot tell which
            // it got. Parsing here is what makes the port's Guids trustworthy.
            var candidate = Path.GetFileNameWithoutExtension(value);

            if (Guid.TryParse(candidate, out var id))
            {
                ids.Add(id);
                continue;
            }

            _logger.LogWarning("Skipped the dataset structure '{Value}': it does not name a dataset id.", value);
        }

        return ids;
    }
}
