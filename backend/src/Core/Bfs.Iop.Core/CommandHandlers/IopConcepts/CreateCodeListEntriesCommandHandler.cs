using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

public sealed class CreateCodeListEntriesCommandHandler : IRequestHandler<CreateCodeListEntriesCommand, IEnumerable<Guid>>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;

    public CreateCodeListEntriesCommandHandler(
        IIopConceptsService iopConceptsService,
        ICodeListEntryIndexService indexService)
    {
        _conceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
    }

    public async Task<IEnumerable<Guid>> Handle(CreateCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        var ids = await _conceptsService.AddCodeListEntries(request.ConceptId, request.InputModels, cancellationToken);

        var concept = await _conceptsService.GetIopConcept(request.ConceptId, includeCodeListEntries: true, cancellationToken);

        _indexService.UpdateIndex(concept.CodeListEntries!);

        return ids;
    }
}


