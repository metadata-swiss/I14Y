using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntryCommandHandler : IRequestHandler<GetCodeListEntryCommand, CodeListEntryModel>
{
    private readonly IIopConceptsService _conceptsService;

    public GetCodeListEntryCommandHandler(IIopConceptsService conceptsService) => 
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task<CodeListEntryModel> Handle(GetCodeListEntryCommand request, CancellationToken cancellationToken) =>
        _conceptsService.GetCodeListEntry(request.ConceptId, request.CodeListEntryId, cancellationToken);
}
