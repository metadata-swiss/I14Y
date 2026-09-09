using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class CreateIopConceptVersionCommandHandler 
    : IRequestHandler<CreateIopConceptVersionCommand, Guid>
{
    private readonly IIopConceptsService _iopConceptsService;
    private readonly ICatalogIndexService _catalogIndexService;

    public CreateIopConceptVersionCommandHandler(
        IIopConceptsService iopConceptsService,
        ICatalogIndexService catalogIndexService)
    {
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task<Guid> Handle(CreateIopConceptVersionCommand request, CancellationToken cancellationToken)
    {
        var id = await _iopConceptsService.AddIopConceptVersion(request.PreviousId, request.Input, cancellationToken);

        var resource = await _iopConceptsService.GetIopConcept(id, false, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        return id;
    }
}
