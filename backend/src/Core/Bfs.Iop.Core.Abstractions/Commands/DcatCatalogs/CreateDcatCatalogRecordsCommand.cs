using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record CreateDcatCatalogRecordsCommand(
    Guid DcatCatalogId, 
    IEnumerable<DcatCatalogRecordInputModel> InputModels) : IRequest<IEnumerable<Guid>>
{ }
