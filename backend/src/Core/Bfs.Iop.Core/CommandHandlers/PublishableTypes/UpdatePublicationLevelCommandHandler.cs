using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal sealed class UpdatePublicationLevelCommandHandler : IRequestHandler<UpdatePublicationLevelCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _iopConceptsService;
    private readonly IMappingTablesService _mappingTablesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public UpdatePublicationLevelCommandHandler(
        IDatasetsService datasetsService,
        IPublicServicesService publicServicesService,
        IDataServicesService dataServicesService,
        IIopConceptsService iopConceptsService,
        IMappingTablesService mappingTablesService,
        ICatalogIndexService catalogIndexService)
    {
        _datasetsService = datasetsService ??
            throw new ArgumentNullException(nameof(datasetsService));

        _publicServicesService = publicServicesService ??
            throw new ArgumentNullException(nameof(publicServicesService));

        _dataServicesService = dataServicesService ??
            throw new ArgumentNullException(nameof(dataServicesService));

        _iopConceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

        _mappingTablesService = mappingTablesService ??
            throw new ArgumentNullException(nameof(mappingTablesService));

        _catalogIndexService = catalogIndexService ?? 
            throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(UpdatePublicationLevelCommand request, CancellationToken cancellationToken)
    {
        var task = request.Type switch
        {
            PublishableResourceType.Dataset => _datasetsService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.PublicService => _publicServicesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.DataService => _dataServicesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.IopConcept =>  _iopConceptsService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.MappingTable => _mappingTablesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };

        await task;

        await UpdateIndex(request.Type, request.Id, cancellationToken);
    }

    private async Task UpdateIndex(PublishableResourceType type, Guid id, CancellationToken cancellationToken)
    {
        switch (type)
        {
            case PublishableResourceType.Dataset:
                _catalogIndexService.UpdateIndex(await _datasetsService.GetDataset(id, cancellationToken));
                break;
            case PublishableResourceType.PublicService:
                _catalogIndexService.UpdateIndex(await _publicServicesService.GetPublicService(id, cancellationToken));
                break;
            case PublishableResourceType.DataService:
                _catalogIndexService.UpdateIndex(await _dataServicesService.GetDataService(id, cancellationToken));
                break;
            case PublishableResourceType.IopConcept:
                _catalogIndexService.UpdateIndex(await _iopConceptsService.GetIopConcept(id, false, cancellationToken));
                break;
            case PublishableResourceType.MappingTable:
                _catalogIndexService.UpdateIndex(await _mappingTablesService.GetMappingTable(id, cancellationToken));
                break;
            default:
                throw new NotSupportedException($"The type '{type}' is not supported.");
        }
    }
}
