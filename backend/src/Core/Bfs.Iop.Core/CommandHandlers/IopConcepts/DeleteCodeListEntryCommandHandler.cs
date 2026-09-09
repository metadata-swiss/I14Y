using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteCodeListEntryCommandHandler : IRequestHandler<DeleteCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;

    public DeleteCodeListEntryCommandHandler(IIopConceptsService conceptsService, ICodeListEntryIndexService indexService)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
    }

    public async Task Handle(DeleteCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.DeleteCodeListEntry(request.ConceptId, request.CodeListEntryId, cancellationToken);

        _indexService.DeIndex([request.CodeListEntryId]);
    }
}
