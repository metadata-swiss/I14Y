using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateIopConceptCommandHandler : IRequestHandler<UpdateIopConceptCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICatalogIndexService _catalogIndexService;

    public UpdateIopConceptCommandHandler(
        IIopConceptsService conceptsService,
        ICatalogIndexService catalogIndexService)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(UpdateIopConceptCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateConcept(request.Id, request.UpdateModel, cancellationToken);

        var resource = await _conceptsService.GetIopConcept(request.Id, false, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
