using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetIopConceptCommandHandler : IRequestHandler<GetIopConceptCommand, IopConceptModel>
{
    private readonly IIopConceptsService _iopConceptsService;

    public GetIopConceptCommandHandler(IIopConceptsService iopConceptsService) => 
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));

    public Task<IopConceptModel> Handle(GetIopConceptCommand request, CancellationToken cancellationToken) =>
        _iopConceptsService.GetIopConcept(request.Id, request.IncludeCodeListEntries, cancellationToken);
}
