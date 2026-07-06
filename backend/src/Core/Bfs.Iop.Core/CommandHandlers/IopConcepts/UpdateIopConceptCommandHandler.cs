using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateIopConceptCommandHandler : IRequestHandler<UpdateIopConceptCommand>
{
    private readonly IIopConceptsService _conceptsService;

    public UpdateIopConceptCommandHandler(IIopConceptsService conceptsService) => 
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task Handle(UpdateIopConceptCommand request, CancellationToken cancellationToken) =>
        _conceptsService.UpdateConcept(request.Id, request.UpdateModel, cancellationToken);
}
