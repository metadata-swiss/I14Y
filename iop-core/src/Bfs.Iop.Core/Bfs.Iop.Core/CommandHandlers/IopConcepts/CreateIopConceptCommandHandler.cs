using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class CreateIopConceptCommandHandler : IRequestHandler<CreateIopConceptCommand, Guid>
{
    private readonly IIopConceptsService _iopConceptsService;

    public CreateIopConceptCommandHandler(IIopConceptsService iopConceptsService) => _iopConceptsService = 
        iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));

    public Task<Guid> Handle(CreateIopConceptCommand request, CancellationToken cancellationToken) => 
        _iopConceptsService.AddIopConcept(request.Input, cancellationToken);
}
