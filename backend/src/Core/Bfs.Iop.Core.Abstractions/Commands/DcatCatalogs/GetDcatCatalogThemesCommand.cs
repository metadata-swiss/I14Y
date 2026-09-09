using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogThemesCommand(
    Guid DcatCatalogId, 
    int? Page,
    int? PageSize) : IRequest<PagedResult<DcatCatalogThemeModel>>
{ }
