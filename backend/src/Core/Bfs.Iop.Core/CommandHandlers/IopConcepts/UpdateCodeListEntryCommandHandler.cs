using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateCodeListEntryCommandHandler : IRequestHandler<UpdateCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;

    public UpdateCodeListEntryCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntryIndexService indexService)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
    }

    public async Task Handle(UpdateCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateCodeListEntry(
            request.ConceptId, 
            request.CodeListEntryId,
            request.UpdateModel,
            cancellationToken);

        var entry = await _conceptsService.GetCodeListEntry(request.ConceptId, request.CodeListEntryId, cancellationToken);

        _indexService.UpdateIndex([entry]);
    }
}
