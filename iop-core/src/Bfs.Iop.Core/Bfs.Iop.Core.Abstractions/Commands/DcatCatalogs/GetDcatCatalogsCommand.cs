using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogsCommand(int? Page, int? PageSize) : IRequest<PagedResult<DcatCatalogModel>>
{ }
