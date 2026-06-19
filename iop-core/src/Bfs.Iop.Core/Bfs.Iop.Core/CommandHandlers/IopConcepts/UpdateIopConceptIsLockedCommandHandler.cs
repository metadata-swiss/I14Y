using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateIopConceptIsLockedCommandHandler : IRequestHandler<UpdateIopConceptIsLockedCommand>
{
    private readonly IIopConceptsService _conceptsService;

    public UpdateIopConceptIsLockedCommandHandler(IIopConceptsService iopConceptsService) => 
        _conceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

    public Task Handle(UpdateIopConceptIsLockedCommand request, CancellationToken cancellationToken)
    {
        return _conceptsService.UpdateIsLocked(request.Id, request.Value, cancellationToken);
    }
}
