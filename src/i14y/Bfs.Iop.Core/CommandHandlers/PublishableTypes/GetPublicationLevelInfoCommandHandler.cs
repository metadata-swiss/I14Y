using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Sis.Dcat.Commands.PublishableTypes;

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
            PublishableType.Dataset => _datasetsService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.DataService => _dataServicesService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.PublicService => _publicServicesService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.IopConcept => _conceptsService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.MappingTable => _mappingTablesService.GetPublicationLevelAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };
}
