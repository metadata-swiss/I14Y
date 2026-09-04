using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal sealed class GetRegistrationStatusInfoCommandHandler : IRequestHandler<GetRegistrationStatusInfoCommand, RegistrationStatusInfoModel>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _conceptsService;
    private readonly IMappingTablesService _mappingTablesService;

    public GetRegistrationStatusInfoCommandHandler(
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

    public Task<RegistrationStatusInfoModel> Handle(GetRegistrationStatusInfoCommand request, CancellationToken cancellationToken) =>
        request.Type switch
        {
            PublishableResourceType.Dataset => _datasetsService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.DataService => _dataServicesService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.PublicService => _publicServicesService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.IopConcept => _conceptsService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableResourceType.MappingTable => _mappingTablesService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };
}
