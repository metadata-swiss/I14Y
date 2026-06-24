using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteCodeListEntryCommandHandler : IRequestHandler<DeleteCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;

    public DeleteCodeListEntryCommandHandler(IIopConceptsService conceptsService) => 
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task Handle(DeleteCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        return _conceptsService.DeleteCodeListEntry(request.ConceptId, request.CodeListEntryId, cancellationToken);
    }
}
