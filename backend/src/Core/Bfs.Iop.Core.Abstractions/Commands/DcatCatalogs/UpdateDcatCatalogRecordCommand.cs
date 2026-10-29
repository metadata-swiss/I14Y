using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record UpdateDcatCatalogRecordCommand(
    Guid DcatCatalogId, 
    Guid DcatCatalogRecordId,
    DcatCatalogRecordInputModel UpdateModel) : IRequest
{ }
