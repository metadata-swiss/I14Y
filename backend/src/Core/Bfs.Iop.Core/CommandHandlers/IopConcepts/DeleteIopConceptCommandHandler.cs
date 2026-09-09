using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteIopConceptCommandHandler : IRequestHandler<DeleteIopConceptCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly ICodeListEntryIndexService _indexService;

    public DeleteIopConceptCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntryIndexService indexService,
        ICatalogIndexService catalogIndexService)
    {
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

        _catalogIndexService = catalogIndexService 
            ?? throw new ArgumentNullException(nameof(catalogIndexService));

        _indexService = indexService 
            ?? throw new ArgumentNullException(nameof(indexService));
    }

    public async Task Handle(DeleteIopConceptCommand request, CancellationToken cancellationToken)
    {
        var concept = await _conceptsService.GetIopConcept(request.Id, true, cancellationToken);

        await _conceptsService.DeleteIopConcept(request.Id, cancellationToken);

        _catalogIndexService.DeIndex(request.Id);

        if (concept.CodeListEntries?.Any() ?? false)
        {
            _indexService.DeIndex(concept.CodeListEntries.Select(x => x.Id));
        }
    }
}
