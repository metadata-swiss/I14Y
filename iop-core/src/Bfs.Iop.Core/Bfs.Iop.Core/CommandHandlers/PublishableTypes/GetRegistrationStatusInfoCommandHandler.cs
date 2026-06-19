using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Sis.Dcat.Commands.PublishableTypes;

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
            PublishableType.Dataset => _datasetsService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.DataService => _dataServicesService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.PublicService => _publicServicesService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.IopConcept => _conceptsService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            PublishableType.MappingTable => _mappingTablesService.GetRegistrationStatusAndProposalAndUserAllowedValues(request.Id, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };
}
