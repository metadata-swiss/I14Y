using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetIopConceptVersionsCommandHandler 
    : IRequestHandler<GetIopConceptVersionsCommand, IEnumerable<IopConceptModel>>
{
    private readonly IIopConceptsService _iopConceptsService;

    public GetIopConceptVersionsCommandHandler(IIopConceptsService iopConceptsService) => 
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));

    public async Task<IEnumerable<IopConceptModel>> Handle(GetIopConceptVersionsCommand request, CancellationToken cancellationToken)
    {
        IopConceptModel? model;

        try
        {
            model = await _iopConceptsService.GetIopConcept(request.Id, false, cancellationToken);
        }
        catch (Exception ex) when (ex is UnauthorizedException or ForbiddenException)
        {
            return [];
        }

        var results = await _iopConceptsService.GetIopConcepts(
            model.Identifiers.First(),
            publisherIdentifier: null,
            version: null,
            publicationLevel: null,
            registrationStatus: null,
            page: 1, pageSize:
            int.MaxValue, cancellationToken);

        return results.Results;
    }
}
