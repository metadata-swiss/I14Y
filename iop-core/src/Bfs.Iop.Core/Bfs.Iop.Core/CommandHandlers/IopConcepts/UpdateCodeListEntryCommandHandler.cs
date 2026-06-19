using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateCodeListEntryCommandHandler : IRequestHandler<UpdateCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;

    public UpdateCodeListEntryCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task Handle(UpdateCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        return _conceptsService.UpdateCodeListEntry(
            request.ConceptId, 
            request.CodeListEntryId,
            request.UpdateModel,
            cancellationToken);
    }
}
