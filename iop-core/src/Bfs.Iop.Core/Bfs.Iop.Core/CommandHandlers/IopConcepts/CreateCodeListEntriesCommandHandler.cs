using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

public sealed class CreateCodeListEntriesCommandHandler : IRequestHandler<CreateCodeListEntriesCommand, IEnumerable<Guid>>
{
    private readonly IIopConceptsService _conceptsService;

    public CreateCodeListEntriesCommandHandler(IIopConceptsService iopConceptsService) =>
        _conceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

    public Task<IEnumerable<Guid>> Handle(CreateCodeListEntriesCommand request, CancellationToken cancellationToken) => 
        _conceptsService.AddCodeListEntries(request.ConceptId, request.InputModels, cancellationToken);
}
