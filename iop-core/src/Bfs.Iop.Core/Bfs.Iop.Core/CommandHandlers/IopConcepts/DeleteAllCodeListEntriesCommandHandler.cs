using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteAllCodeListEntriesCommandHandler : IRequestHandler<DeleteAllCodeListEntriesCommand>
{
    private readonly IIopConceptsService _conceptsService;

    public DeleteAllCodeListEntriesCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

    public Task Handle(DeleteAllCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        return _conceptsService.DeleteAllCodeListEntriesFromIopConcept(request.ConceptId, cancellationToken);
    }
}
