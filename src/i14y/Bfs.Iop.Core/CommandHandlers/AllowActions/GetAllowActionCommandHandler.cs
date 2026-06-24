using Bfs.Iop.Core.Abstractions.Commands.AllowActions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.AllowActions;

internal sealed class GetAllowActionCommandHandler : IRequestHandler<GetAllowActionCommand, IEnumerable<AllowActionResult>>
{
    private readonly IDataServicesService _dataServicesService;
    private readonly IDatasetsService _datasetsService;
    private readonly IDcatCatalogRecordsService _dcatCatalogRecordsService;
    private readonly IIopConceptsService _conceptsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IMappingTablesService _mappingTablesService;

    public GetAllowActionCommandHandler(
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

    public Task<IEnumerable<AllowActionResult>> Handle(GetAllowActionCommand request, CancellationToken cancellationToken)
    {
        var task = request.ResourceType switch
        {
            AllowActionResourceType.Dataset => _datasetsService.GetUserAllowActionInfo(request.ResourceId, cancellationToken),
            AllowActionResourceType.DataService => _dataServicesService.GetUserAllowActionInfo(request.ResourceId, cancellationToken),
            AllowActionResourceType.DcatCatalogRecord => _dcatCatalogRecordsService.GetUserAllowActionInfo(request.ResourceId, cancellationToken),
            AllowActionResourceType.Concept => _conceptsService.GetUserAllowActionInfo(request.ResourceId, cancellationToken),
            AllowActionResourceType.PublicService => _publicServicesService.GetUserAllowActionInfo(request.ResourceId, cancellationToken),
            AllowActionResourceType.MappingTable => _mappingTablesService.GetUserAllowActionInfo(request.ResourceId, cancellationToken),
            _ => throw new NotSupportedException($"The resource type '{request.ResourceType}' is not supported.")
        };

        return task;
    }
}
