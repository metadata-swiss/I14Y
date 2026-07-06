using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.Geocat.Search;

public sealed class GeocatSearchCommand : IRequest<PagedResult<MetaSearchResultItem>>
{
    public required string Culture { get; set; }

    public required int Page { get; set; }

    public required int PageSize { get; set; }

    public string? Query { get; set; }
}