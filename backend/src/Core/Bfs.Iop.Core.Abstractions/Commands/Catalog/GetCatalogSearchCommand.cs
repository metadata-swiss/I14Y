using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Catalog;

public sealed record GetCatalogSearchCommand(
    string? Query,
    string? Language,
    CatalogSearchFilter Filter,
    int? Page, 
    int? PageSize) : IRequest<PagedResult<SearchResultModel>>
{ }
