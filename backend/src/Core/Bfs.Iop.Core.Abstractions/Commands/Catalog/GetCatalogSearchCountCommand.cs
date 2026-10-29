using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Catalog;

public sealed record GetCatalogSearchCountCommand(
    string? QueryString,
    string? Language, 
    CatalogSearchFilter Filter) : IRequest<SearchCountResultModel>
{ }
