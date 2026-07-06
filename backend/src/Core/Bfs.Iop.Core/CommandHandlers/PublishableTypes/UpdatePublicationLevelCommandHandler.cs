using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal sealed class UpdatePublicationLevelCommandHandler : IRequestHandler<UpdatePublicationLevelCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _iopConceptsService;
    private readonly IMappingTablesService _mappingTablesService;

    public UpdatePublicationLevelCommandHandler(
        IDatasetsService datasetsService,
        IPublicServicesService publicServicesService,
        IDataServicesService dataServicesService,
        IIopConceptsService iopConceptsService,
        IMappingTablesService mappingTablesService)
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
    }

    public Task Handle(UpdatePublicationLevelCommand request, CancellationToken cancellationToken)
    {
        var task = request.Type switch
        {
            PublishableType.Dataset => _datasetsService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableType.PublicService => _publicServicesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableType.DataService => _dataServicesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableType.IopConcept =>  _iopConceptsService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableType.MappingTable => _mappingTablesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };

        return task;
    }
}
