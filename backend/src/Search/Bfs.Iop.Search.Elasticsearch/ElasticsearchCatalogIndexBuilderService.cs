using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Search.Abstractions;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Full (re)build of the Elasticsearch catalog index. Mirrors the Lucene
/// <c>CatalogIndexBuilderService</c>: it pulls the same domain models in batches and pushes them
/// through <see cref="ICatalogIndexService"/> — which, under the ES engine, is the Elasticsearch
/// implementation. Codelist-entry indexing is out of scope for the PoC.
/// </summary>
internal sealed class ElasticsearchCatalogIndexBuilderService : IIndexBuilderService
{
    private const int DefaultBatchSize = 100;

    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IIopConceptsService _conceptsService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IDatasetsService _datasetsService;
    private readonly ILogger<ElasticsearchCatalogIndexBuilderService> _logger;
    private readonly IMappingTablesService _mappingTablesService;
    private readonly IPublicServicesService _publicServicesService;

    public ElasticsearchCatalogIndexBuilderService(
        ILogger<ElasticsearchCatalogIndexBuilderService> logger,
        IDataServicesService dataServicesService,
        ICatalogIndexService catalogIndexService,
        IPublicServicesService publicServicesService,
        IIopConceptsService conceptsService,
        IDatasetsService datasetsService,
        IMappingTablesService mappingTablesService,
        IDatasetModelProcessService datasetModelFileProcessService)
    {
        _logger = logger;
        _dataServicesService = dataServicesService;
        _catalogIndexService = catalogIndexService;
        _publicServicesService = publicServicesService;
        _conceptsService = conceptsService;
        _datasetsService = datasetsService;
        _mappingTablesService = mappingTablesService;
        _datasetModelFileProcessService = datasetModelFileProcessService;
    }

    public async Task BuildIndexAsync(CancellationToken cancellationToken = default)
    {
        // Sequential: the scoped DbContext cannot be used concurrently.
        await IndexDatasets(cancellationToken);
        await IndexDataServices(cancellationToken);
        await IndexPublicServices(cancellationToken);
        await IndexConcepts(cancellationToken);
        await IndexMappingTables(cancellationToken);
    }

    private async Task IndexDatasets(CancellationToken cancellationToken)
    {
        var count = 0;
        IEnumerable<string> datasetIds;
        try
        {
            datasetIds = await _datasetModelFileProcessService.GetAllDatasetIdsWithStructures(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read dataset structures.");
            datasetIds = [];
        }

        try
        {
            await foreach (var models in _datasetsService.GetDatasetsForIndexInBatches(DefaultBatchSize, cancellationToken))
            {
                var datasets = models.ToArray();
                await _catalogIndexService.UpdateIndexAsync(datasets, datasetIds, cancellationToken);
                count += datasets.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while indexing datasets into Elasticsearch.");
        }

        _logger.LogInformation("{Count} datasets indexed into Elasticsearch.", count);
    }

    private async Task IndexDataServices(CancellationToken cancellationToken)
    {
        var count = 0;
        try
        {
            await foreach (var models in _dataServicesService.GetDataServicesForIndexInBatches(DefaultBatchSize, cancellationToken))
            {
                var dataServices = models.ToArray();
                await _catalogIndexService.UpdateIndexAsync(dataServices, cancellationToken);
                count += dataServices.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while indexing data services into Elasticsearch.");
        }

        _logger.LogInformation("{Count} data services indexed into Elasticsearch.", count);
    }

    private async Task IndexPublicServices(CancellationToken cancellationToken)
    {
        var count = 0;
        try
        {
            await foreach (var models in _publicServicesService.GetPublicServicesForIndexInBatches(DefaultBatchSize, cancellationToken))
            {
                var publicServices = models.ToArray();
                await _catalogIndexService.UpdateIndexAsync(publicServices, cancellationToken);
                count += publicServices.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while indexing public services into Elasticsearch.");
        }

        _logger.LogInformation("{Count} public services indexed into Elasticsearch.", count);
    }

    private async Task IndexConcepts(CancellationToken cancellationToken)
    {
        var count = 0;
        try
        {
            await foreach (var models in _conceptsService.GetIopConceptsForIndexInBatches(DefaultBatchSize, cancellationToken))
            {
                var concepts = models.ToArray();
                await _catalogIndexService.UpdateIndexAsync(concepts, cancellationToken);
                count += concepts.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while indexing concepts into Elasticsearch.");
        }

        _logger.LogInformation("{Count} concepts indexed into Elasticsearch.", count);
    }

    private async Task IndexMappingTables(CancellationToken cancellationToken)
    {
        var count = 0;
        try
        {
            await foreach (var models in _mappingTablesService.GetMappingTablesForIndexInBatches(DefaultBatchSize, cancellationToken))
            {
                var mappingTables = models.ToArray();
                await _catalogIndexService.UpdateIndexAsync(mappingTables, cancellationToken);
                count += mappingTables.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while indexing mapping tables into Elasticsearch.");
        }

        _logger.LogInformation("{Count} mapping tables indexed into Elasticsearch.", count);
    }
}
