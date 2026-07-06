using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteIopConceptCommandHandler : IRequestHandler<DeleteIopConceptCommand>
{
    private readonly IIopConceptsService _conceptsService;

    public DeleteIopConceptCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

    public Task Handle(DeleteIopConceptCommand request, CancellationToken cancellationToken)
    {
        return _conceptsService.DeleteIopConcept(request.Id, cancellationToken);
    }
}
