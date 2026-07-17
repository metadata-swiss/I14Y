using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Lucene.IndexBuilders;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Elasticsearch;

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

    public async Task BuildIndex(CancellationToken cancellationToken = default)
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

        await foreach (var models in _datasetsService.GetDatasetsForIndexInBatches(DefaultBatchSize, cancellationToken))
        {
            var datasets = models.ToArray();
            _catalogIndexService.UpdateIndex(datasets, datasetIds);
            count += datasets.Length;
        }

        _logger.LogInformation("{Count} datasets indexed into Elasticsearch.", count);
    }

    private async Task IndexDataServices(CancellationToken cancellationToken)
    {
        var count = 0;
        await foreach (var models in _dataServicesService.GetDataServicesForIndexInBatches(DefaultBatchSize, cancellationToken))
        {
            var dataServices = models.ToArray();
            _catalogIndexService.UpdateIndex(dataServices);
            count += dataServices.Length;
        }

        _logger.LogInformation("{Count} data services indexed into Elasticsearch.", count);
    }

    private async Task IndexPublicServices(CancellationToken cancellationToken)
    {
        var count = 0;
        await foreach (var models in _publicServicesService.GetPublicServicesForIndexInBatches(DefaultBatchSize, cancellationToken))
        {
            var publicServices = models.ToArray();
            _catalogIndexService.UpdateIndex(publicServices);
            count += publicServices.Length;
        }

        _logger.LogInformation("{Count} public services indexed into Elasticsearch.", count);
    }

    private async Task IndexConcepts(CancellationToken cancellationToken)
    {
        var count = 0;
        await foreach (var models in _conceptsService.GetIopConceptsForIndexInBatches(DefaultBatchSize, cancellationToken))
        {
            var concepts = models.ToArray();
            _catalogIndexService.UpdateIndex(concepts);
            count += concepts.Length;
        }

        _logger.LogInformation("{Count} concepts indexed into Elasticsearch.", count);
    }

    private async Task IndexMappingTables(CancellationToken cancellationToken)
    {
        var count = 0;
        await foreach (var models in _mappingTablesService.GetMappingTablesForIndexInBatches(DefaultBatchSize, cancellationToken))
        {
            var mappingTables = models.ToArray();
            _catalogIndexService.UpdateIndex(mappingTables);
            count += mappingTables.Length;
        }

        _logger.LogInformation("{Count} mapping tables indexed into Elasticsearch.", count);
    }
}
