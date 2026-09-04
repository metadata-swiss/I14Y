using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetIopConceptsCommandHandler : IRequestHandler<GetIopConceptsCommand, PagedResult<IopConceptModel>>
{
    private readonly IIopConceptsService _conceptsService;

    public GetIopConceptsCommandHandler(IIopConceptsService conceptsService) => 
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task<PagedResult<IopConceptModel>> Handle(GetIopConceptsCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _conceptsService.GetIopConcepts(
            request.ConceptIdentifier,
            request.PublisherIdentifier,
            request.Version,
            request.PublicationLevel,
            request.RegistrationStatus,
            page,
            pageSize,
            cancellationToken);
    }
}
