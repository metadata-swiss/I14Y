using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Lucene.IndexBuilders;

internal sealed class CatalogIndexBuilderService : IIndexBuilderService
{
    private const int DefaultBatchSize = 100;

    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly ILogger<CatalogIndexBuilderService> _logger;
    private readonly ISearchIndexProviderService _indexProviderService;

    public CatalogIndexBuilderService(
        ILogger<CatalogIndexBuilderService> logger,
        ICatalogIndexService catalogIndexService,
        IDatasetModelProcessService datasetModelFileProcessService,
        ISearchIndexProviderService indexProviderService)
    {
        _logger = logger;
        _catalogIndexService = catalogIndexService;
        _datasetModelFileProcessService = datasetModelFileProcessService;
        _indexProviderService = indexProviderService;
    }

    public async Task BuildIndex(CancellationToken cancellationToken = default)
    {
        // They must be done sequentially, otherwise the Scoped DbContext won't like it.
        await IndexDatasets(cancellationToken);
        await IndexDataServices(cancellationToken);
        await IndexPublicServices(cancellationToken);
        await IndexIopConcepts(cancellationToken);
        await IndexMappingTables(cancellationToken);
    }

    private async Task IndexDataServices(CancellationToken cancellationToken = default)
    {
        int count = 0;

        _logger.LogInformation("Start building DataServices index.");

        await foreach (var models in _indexProviderService.GetDataServicesInBatches(DefaultBatchSize, cancellationToken))
        {
            try
            {
                var dataServices = models.ToArray();
                _catalogIndexService.UpdateIndex(dataServices);
                count += dataServices.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A problem occurred while indexing DataServices.");
                return;
            }
        }

        _logger.LogInformation("{Count} DataServices indexed successfully.", count);
    }

    private async Task IndexDatasets(CancellationToken cancellationToken = default)
    {
        int count = 0;
        IEnumerable<string> datasetIds;

        _logger.LogInformation("Start building Datasets index.");

        try
        {
            datasetIds = await _datasetModelFileProcessService.GetAllDatasetIdsWithStructures(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while getting the structured from Datasets.");
            datasetIds = [];
        }

        await foreach (var models in _indexProviderService.GetDatasetsInBatches(DefaultBatchSize, cancellationToken))
        {
            try
            {
                var datasets = models.ToArray();
                _catalogIndexService.UpdateIndex(datasets, datasetIds);
                count += datasets.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A problem occurred while indexing Datasets.");
                return;
            }
        }

        _logger.LogInformation("{Count} Datasets indexed successfully.", count);
    }

    private async Task IndexIopConcepts(CancellationToken cancellationToken = default)
    {
        int count = 0;

        _logger.LogInformation("Start building IopConcepts index.");

        await foreach (var models in _indexProviderService.GetIopConceptsInBatches(DefaultBatchSize, cancellationToken))
        {
            try
            {
                var concepts = models.ToArray();
                _catalogIndexService.UpdateIndex(concepts);
                count += concepts.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A problem occurred while indexing IopConcepts.");
                return;
            }
        }

        _logger.LogInformation("{Count} IopConcepts indexed successfully.", count);
    }

    private async Task IndexMappingTables(CancellationToken cancellationToken = default)
    {
        int count = 0;

        _logger.LogInformation("Start building Mapping tables index.");

        await foreach (var models in _indexProviderService.GetMappingTablesInBatches(DefaultBatchSize, cancellationToken))
        {
            try
            {
                var mappingTables = models.ToArray();
                _catalogIndexService.UpdateIndex(mappingTables);
                count += mappingTables.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A problem occurred while indexing Mapping tables.");
                return;
            }
        }

        _logger.LogInformation("{Count} Mapping tables indexed successfully.", count);
    }

    private async Task IndexPublicServices(CancellationToken cancellationToken = default)
    {
        int count = 0;

        _logger.LogInformation("Start building PublicServices index.");

        await foreach (var models in _indexProviderService.GetPublicServicesInBatches(DefaultBatchSize, cancellationToken))
        {
            try
            {
                var publicServices = models.ToArray();
                _catalogIndexService.UpdateIndex(publicServices);
                count += publicServices.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A problem occurred while indexing PublicServices.");
                return;
            }
        }

        _logger.LogInformation("{Count} PublicServices indexed successfully.", count);
    }
}