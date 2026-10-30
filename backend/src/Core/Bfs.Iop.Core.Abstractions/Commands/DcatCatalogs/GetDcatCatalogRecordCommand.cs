using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogRecordCommand(Guid DcatCatalogId, Guid DcatCatalogRecordId) : IRequest<DcatCatalogRecordModel>
{ }
