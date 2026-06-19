using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class CreateIopConceptVersionCommandHandler 
    : IRequestHandler<CreateIopConceptVersionCommand, Guid>
{
    private readonly IIopConceptsService _iopConceptsService;

    public CreateIopConceptVersionCommandHandler(IIopConceptsService iopConceptsService) => _iopConceptsService = 
        iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));

    public Task<Guid> Handle(CreateIopConceptVersionCommand request, CancellationToken cancellationToken) =>
        _iopConceptsService.AddIopConceptVersion(request.PreviousId, request.Input, cancellationToken);
}
