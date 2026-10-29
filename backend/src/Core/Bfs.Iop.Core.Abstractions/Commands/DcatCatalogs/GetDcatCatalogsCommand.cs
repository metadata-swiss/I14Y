using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogsCommand(int? Page, int? PageSize) : IRequest<PagedResult<DcatCatalogModel>>
{ }
