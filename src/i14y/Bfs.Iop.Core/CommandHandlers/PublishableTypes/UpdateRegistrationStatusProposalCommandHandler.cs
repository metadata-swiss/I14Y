using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal sealed class UpdateRegistrationStatusProposalCommandHandler : IRequestHandler<UpdateRegistrationStatusProposalCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _iopConceptsService;
    private readonly IMappingTablesService _mappingTablesService;

    public UpdateRegistrationStatusProposalCommandHandler(
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

    public Task Handle(UpdateRegistrationStatusProposalCommand request, CancellationToken cancellationToken)
    {
        var task = request.Type switch
        {
            PublishableType.Dataset => _datasetsService.UpdateRegistrationStatusProposal(request.Id, request.Proposal, cancellationToken),
            PublishableType.PublicService => _publicServicesService.UpdateRegistrationStatusProposal(request.Id, request.Proposal, cancellationToken),
            PublishableType.DataService => _dataServicesService.UpdateRegistrationStatusProposal(request.Id, request.Proposal, cancellationToken),
            PublishableType.IopConcept => _iopConceptsService.UpdateRegistrationStatusProposal(request.Id, request.Proposal, cancellationToken),
            PublishableType.MappingTable => _iopConceptsService.UpdateRegistrationStatusProposal(request.Id, request.Proposal, cancellationToken),
            _ => throw new NotSupportedException($"The type '{request.Type}' is not supported.")
        };

        return task;
    }
}
