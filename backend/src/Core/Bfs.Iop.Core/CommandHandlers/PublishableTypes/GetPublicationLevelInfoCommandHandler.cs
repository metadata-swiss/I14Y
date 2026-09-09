using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal sealed class GetPublicationLevelInfoCommandHandler : IRequestHandler<GetPublicationLevelInfoCommand, PublicationLevelInfoModel>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _conceptsService;
    private readonly IMappingTablesService _mappingTablesService;

    public GetPublicationLevelInfoCommandHandler(
        IDatasetsService datasetsService,
        IPublicServicesService publicServicesService,
        IDataServicesService dataServicesService,
        IIopConceptsService conceptsService,
        IMappingTablesService mappingTablesService)
    {
        _datasetsService = datasetsService ??
            throw new ArgumentNullException(nameof(datasetsService));

        _publicServicesService = publicServicesService ??
            throw new ArgumentNullException(nameof(publicServicesService));

        _dataServicesService = dataServicesService ??
            throw new ArgumentNullException(nameof(dataServicesService));

        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

        _mappingTablesService = mappingTablesService ??
            throw new ArgumentNullException(nameof(mappingTablesService));
    }

    public Task<PublicationLevelInfoModel> Handle(GetPublicationLevelInfoCommand request, CancellationToken cancellationToken) => 
        request.Type switch
        {
            PublishableResourceType.Dataset => _datasetsService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.DataService => _dataServicesService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.PublicService => _publicServicesService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.IopConcept => _conceptsService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.MappingTable => _mappingTablesService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };
}
