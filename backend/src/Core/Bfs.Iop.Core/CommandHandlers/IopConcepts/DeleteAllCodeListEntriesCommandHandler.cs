using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteAllCodeListEntriesCommandHandler : IRequestHandler<DeleteAllCodeListEntriesCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;

    public DeleteAllCodeListEntriesCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntryIndexService indexService)
    {
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
    }

    public async Task Handle(DeleteAllCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        var ids = (await _conceptsService.GetCodeListEntries(
            request.ConceptId,
            sortProperty: null,
            page: 1,
            pageSize: int.MaxValue,
            sortOrder: DataAccess.Abstractions.SortOrder.Ascending, 
            cancellationToken: cancellationToken)).Results.Select(x => x.Id);

        await _conceptsService.DeleteAllCodeListEntriesFromIopConcept(request.ConceptId, cancellationToken);

        _indexService.DeIndex(ids);
    }
}
