using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

using CatalogSearchSort = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchSort;

namespace Bfs.Iop.Core.Abstractions.Commands.Catalog;

public sealed record GetCatalogSearchCommand(
    string? Query,
    string? Language,
    CatalogSearchFilter Filter,
    CatalogSearchSort Sort,
    int? Page,
    int? PageSize) : IRequest<PagedResult<SearchResultModel>>
{ }
