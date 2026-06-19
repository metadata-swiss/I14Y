using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record DeleteDcatCatalogRecordCommand(Guid DcatCatalogId, Guid DcatCatalogRecordId) : IRequest
{ }
