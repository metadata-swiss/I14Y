using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
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
    private readonly IPublishableResourceNotifier _notifier;

    public UpdatePublicationLevelCommandHandler(
        IDatasetsService datasetsService,
        IPublicServicesService publicServicesService,
        IDataServicesService dataServicesService,
        IIopConceptsService iopConceptsService,
        IMappingTablesService mappingTablesService,
        IPublishableResourceNotifier notifier)
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

        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public async Task Handle(UpdatePublicationLevelCommand request, CancellationToken cancellationToken)
    {
        var task = request.Type switch
        {
            PublishableResourceType.Dataset => _datasetsService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.PublicService => _publicServicesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.DataService => _dataServicesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.IopConcept => _iopConceptsService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            PublishableResourceType.MappingTable => _mappingTablesService.UpdatePublicationLevel(request.Id, request.Level, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };

        await task;

        await _notifier.NotifyUpdatedAsync(request.Type, request.Id, cancellationToken);
    }
}
