using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateIopConceptIsLockedCommandHandler : IRequestHandler<UpdateIopConceptIsLockedCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICatalogIndexService _catalogIndexService;

    public UpdateIopConceptIsLockedCommandHandler(
        IIopConceptsService iopConceptsService,
        ICatalogIndexService catalogIndexService)
    {
        _conceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

        _catalogIndexService = catalogIndexService 
            ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(UpdateIopConceptIsLockedCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateIsLocked(request.Id, request.Value, cancellationToken);

        var resource = await _conceptsService.GetIopConcept(request.Id, false, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
