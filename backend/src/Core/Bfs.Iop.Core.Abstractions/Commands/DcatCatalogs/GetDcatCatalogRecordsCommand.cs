using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogRecordsCommand(
    Guid DcatCatalogId,
    int? Page,
    int? PageSize) : IRequest<PagedResult<DcatCatalogRecordModel>>
{ }
