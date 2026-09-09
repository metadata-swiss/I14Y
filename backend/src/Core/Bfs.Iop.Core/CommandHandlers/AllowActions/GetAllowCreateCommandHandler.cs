using Bfs.Iop.Core.Abstractions.Commands.AllowActions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.AllowActions;

internal sealed class GetAllowCreateCommandHandler : IRequestHandler<GetAllowCreateCommand, IEnumerable<AllowActionResult>>
{
    private readonly IDataServicesService _dataServicesService;
    private readonly IDatasetsService _datasetsService;
    private readonly IDcatCatalogRecordsService _dcatCatalogRecordsService;
    private readonly IIopConceptsService _conceptsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IMappingTablesService _mappingTablesService;

    public GetAllowCreateCommandHandler(
        IDataServicesService dataServicesService,
        IDatasetsService datasetsService,
        IDcatCatalogRecordsService dcatCatalogRecordsService,
        IIopConceptsService conceptsService,
        IPublicServicesService publicServicesService,
        IMappingTablesService mappingTablesService)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _dcatCatalogRecordsService = dcatCatalogRecordsService ?? throw new ArgumentNullException(nameof(dcatCatalogRecordsService));
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
    }

    public async Task<IEnumerable<AllowActionResult>> Handle(GetAllowCreateCommand request, CancellationToken cancellationToken)
    {
        var tasks = Enum
            .GetValues<AllowActionResourceType>()
            .Select(x => GetAllowCreateInfo(x, cancellationToken));

        return await Task.WhenAll(tasks);
    }

    private async Task<AllowActionResult> GetAllowCreateInfo(AllowActionResourceType resourceType, CancellationToken cancellationToken)
    {
        var task = resourceType switch
        {
            AllowActionResourceType.Dataset => _datasetsService.GetUserAllowCreateInfo(cancellationToken),
            AllowActionResourceType.DataService => _dataServicesService.GetUserAllowCreateInfo(cancellationToken),
            AllowActionResourceType.DcatCatalogRecord => _dcatCatalogRecordsService.GetUserAllowCreateInfo(cancellationToken),
            AllowActionResourceType.Concept => _conceptsService.GetUserAllowCreateInfo(cancellationToken),
            AllowActionResourceType.PublicService => _publicServicesService.GetUserAllowCreateInfo(cancellationToken),
            AllowActionResourceType.MappingTable => _mappingTablesService.GetUserAllowCreateInfo(cancellationToken),
            _ => throw new NotSupportedException($"The resource type '{resourceType}' is not supported.")
        };

        var result = await task;

        return result with
        {
            ResourceType = resourceType
        };
    }
}
