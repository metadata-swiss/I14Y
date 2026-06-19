using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntryByCodeCommandHandler : IRequestHandler<GetCodeListEntryByCodeCommand, CodeListEntryModel>
{
    private readonly IIopConceptsService _conceptsService;

    public GetCodeListEntryByCodeCommandHandler(IIopConceptsService conceptsService) => _conceptsService = 
        conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task<CodeListEntryModel> Handle(GetCodeListEntryByCodeCommand request, CancellationToken cancellationToken) =>
        _conceptsService.GetCodeListEntryByCode(request.ConceptId, request.Code, cancellationToken);
}
