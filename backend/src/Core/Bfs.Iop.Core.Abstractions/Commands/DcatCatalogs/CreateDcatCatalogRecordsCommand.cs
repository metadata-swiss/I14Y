using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record CreateDcatCatalogRecordsCommand(
    Guid DcatCatalogId, 
    IEnumerable<DcatCatalogRecordInputModel> InputModels) : IRequest<IEnumerable<Guid>>
{ }
