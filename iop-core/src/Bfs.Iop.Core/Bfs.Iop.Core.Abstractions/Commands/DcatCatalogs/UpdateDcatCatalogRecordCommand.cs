using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record UpdateDcatCatalogRecordCommand(
    Guid DcatCatalogId, 
    Guid DcatCatalogRecordId,
    DcatCatalogRecordInputModel UpdateModel) : IRequest
{ }
