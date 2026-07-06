using Bfs.Iop.Core.Abstractions.Commands.IopPersons;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopPersons;

internal class SearchIopPersonsByQueryCommandHandler : 
    IRequestHandler<SearchIopPersonsByQueryCommand, PagedResult<IopPersonModel>>
{
    private readonly IIopPersonsService _iopPersonService;

    public SearchIopPersonsByQueryCommandHandler(
        IIopPersonsService iopPersonService) => _iopPersonService = iopPersonService;

    public async Task<PagedResult<IopPersonModel>> Handle(
        SearchIopPersonsByQueryCommand request,
        CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return await _iopPersonService.SearchIopPersons(
            request.Query,
            page,
            pageSize,
            cancellationToken);
    }
}