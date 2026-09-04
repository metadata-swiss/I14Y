using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;
internal sealed class GetIopConceptsByIdentifierCommandHandler 
    : IRequestHandler<GetIopConceptsByIdentifierCommand, IEnumerable<IopConceptModel>>
{
    private readonly IIopConceptsService _conceptsService;

    public GetIopConceptsByIdentifierCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

    public async Task<IEnumerable<IopConceptModel>> Handle(GetIopConceptsByIdentifierCommand request, CancellationToken cancellationToken)
    {
        var results = await _conceptsService.GetIopConcepts(
            request.Identifier, 
            publisherIdentifier: null, 
            version: null,
            publicationLevel: null,
            registrationStatus: null, 
            page: 1, 
            pageSize: int.MaxValue,
            cancellationToken);

        return results.Results;
    }
}
