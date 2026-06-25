using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DcatCatalogRecords;

public record DeleteDcatCatalogRecordCommand(Guid CatalogId, Guid CatalogRecordId) : IRequest
{ }