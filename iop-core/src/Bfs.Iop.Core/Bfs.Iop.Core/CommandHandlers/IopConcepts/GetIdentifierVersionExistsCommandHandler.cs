using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetIdentifierVersionExistsCommandHandler : IRequestHandler<GetIdentifierVersionExistsCommand, IdentifierVersionExistsModel>
{
    private readonly IIopConceptsService _iopConceptsService;

    public GetIdentifierVersionExistsCommandHandler(IIopConceptsService iopConceptsService) => 
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));

    public Task<IdentifierVersionExistsModel> Handle(GetIdentifierVersionExistsCommand request, CancellationToken cancellationToken)
    {
        // we need to have the information about objects that user have no rights to see.
        return _iopConceptsService.GetIdentifierVersionExists(request.Identifier, request.Version, cancellationToken);
    }
}
