using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogsCommand(
    string? PublisherIdentifier, 
    string? TenantIdentifier,
    int? Page, 
    int? PageSize) : IRequest<PagedResult<DcatCatalogModel>>
{ }
