using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

internal sealed class GetRelationsCountCommandHandler
    : IRequestHandler<GetRelationsCountCommand, IReadOnlyList<RelationsCountModel>>
{
    private readonly IRelationsCountService _relatedByCountService;

    public GetRelationsCountCommandHandler(IRelationsCountService relatedByCountService)
    {
        _relatedByCountService = relatedByCountService ??
            throw new ArgumentNullException(nameof(relatedByCountService));
    }

    public async Task<IReadOnlyList<RelationsCountModel>> Handle(
        GetRelationsCountCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var items = request.Items ?? [];

        if (items.Count == 0)
        {
            return [];
        }

        var conceptIds = IdsOfType(items, SearchResourceType.Concept);
        var datasetIds = IdsOfType(items, SearchResourceType.Dataset);
        var dataServiceIds = IdsOfType(items, SearchResourceType.DataService);
        var publicServiceIds = IdsOfType(items, SearchResourceType.PublicService);
        var mappingTableIds = IdsOfType(items, SearchResourceType.MappingTable);

        // All queries below run sequentially: they share a single scoped DbContext (not thread-safe)
        // and the concept structure count issues a single SPARQL query. Each count already applies
        // the caller's read-authorization inside the service.
        var structureCounts = conceptIds.Count > 0
            ? await _relatedByCountService.GetConceptStructureReferenceCountBatch(conceptIds, cancellationToken)
            : EmptyCounts;

        var conceptMappingCounts = conceptIds.Count > 0
            ? await _relatedByCountService.GetConceptMappingTableCountBatch(conceptIds, cancellationToken)
            : EmptyCounts;

        var datasetDataServiceCounts =
            await _relatedByCountService.GetDatasetServedByDataServiceCountBatch(datasetIds, cancellationToken);

        var dataServiceDatasetCounts =
            await _relatedByCountService.GetDataServiceReferencedByDatasetCountBatch(dataServiceIds, cancellationToken);

        var publicServiceCounts =
            await _relatedByCountService.GetPublicServiceReferencedByCountBatch(publicServiceIds, cancellationToken);

        var publicServiceDatasetCounts =
            await _relatedByCountService.GetPublicServiceDescribesDatasetCountBatch(publicServiceIds, cancellationToken);

        var mappingTableCounts =
            await _relatedByCountService.GetMappingTableReferencedByCountBatch(mappingTableIds, cancellationToken);

        return items
            .Select(item => item.Type switch
            {
                SearchResourceType.Concept => BuildConcept(item.Id, structureCounts, conceptMappingCounts),
                SearchResourceType.Dataset => BuildDataset(item.Id, datasetDataServiceCounts),
                SearchResourceType.DataService => BuildDataServiceCount(item.Id, dataServiceDatasetCounts),
                SearchResourceType.PublicService => BuildPublicServiceCount(item.Id, publicServiceCounts, publicServiceDatasetCounts),
                SearchResourceType.MappingTable => BuildTotalOnly(item.Id, mappingTableCounts),
                _ => new RelationsCountModel { Id = item.Id, Total = 0 },
            })
            .ToList();
    }

    private static List<Guid> IdsOfType(IReadOnlyList<RelationsCountRequestItem> items, SearchResourceType type) =>
        items.Where(x => x.Type == type).Select(x => x.Id).Distinct().ToList();

    private static RelationsCountModel BuildConcept(
        Guid id,
        IReadOnlyDictionary<Guid, int> structureCounts,
        IReadOnlyDictionary<Guid, int> mappingCounts)
    {
        var structureAttribute = structureCounts.GetValueOrDefault(id);
        var mappingTable = mappingCounts.GetValueOrDefault(id);

        return new RelationsCountModel
        {
            Id = id,
            StructureAttribute = structureAttribute,
            MappingTable = mappingTable,
            Total = structureAttribute + mappingTable,
        };
    }

    private static RelationsCountModel BuildDataset(Guid id, IReadOnlyDictionary<Guid, int> dataServiceCounts)
    {
        // Matches the dataset detail page, which shows only "Is served by" (the data services serving
        // it). Public services describing the dataset are not rendered there, so they are not counted.
        var dataservice = dataServiceCounts.GetValueOrDefault(id);

        return new RelationsCountModel
        {
            Id = id,
            DataService = dataservice,
            Total = dataservice,
        };
    }

    private static RelationsCountModel BuildDataServiceCount(Guid id, IReadOnlyDictionary<Guid, int> counts)
    {
        var count = counts.GetValueOrDefault(id);
        return new RelationsCountModel { Id = id, Dataset = count, Total = count };
    }

    private static RelationsCountModel BuildPublicServiceCount(
        Guid id,
        IReadOnlyDictionary<Guid, int> serviceCounts,
        IReadOnlyDictionary<Guid, int> datasetCounts)
    {
        // Matches the public service detail Relations section: "Is linked to" + "Requires"
        // (services) and "Is described at" (datasets).
        var services = serviceCounts.GetValueOrDefault(id);
        var datasets = datasetCounts.GetValueOrDefault(id);

        return new RelationsCountModel
        {
            Id = id,
            PublicService = services,
            Dataset = datasets,
            Total = services + datasets,
        };
    }

    private static RelationsCountModel BuildTotalOnly(Guid id, IReadOnlyDictionary<Guid, int> counts)
    {
        var count = counts.GetValueOrDefault(id);
        return new RelationsCountModel { Id = id, Total = count };
    }

    private static IReadOnlyDictionary<Guid, int> EmptyCounts { get; } = new Dictionary<Guid, int>();
}
