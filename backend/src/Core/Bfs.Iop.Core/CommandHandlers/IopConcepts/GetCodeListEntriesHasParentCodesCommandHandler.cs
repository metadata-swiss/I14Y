using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesHasParentCodesCommandHandler : IRequestHandler<GetCodeListEntriesHasParentCodesCommand, bool>
{
    private readonly IIopConceptsService _conceptsService;

    public GetCodeListEntriesHasParentCodesCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task<bool> Handle(GetCodeListEntriesHasParentCodesCommand request, CancellationToken cancellationToken)
            => _conceptsService.GetCodeListEntriesHasParentCodes(
            request.ConceptId,
            cancellationToken);
}
