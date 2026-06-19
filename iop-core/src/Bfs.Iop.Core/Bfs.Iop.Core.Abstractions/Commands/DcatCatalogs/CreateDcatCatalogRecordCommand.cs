using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record CreateDcatCatalogRecordCommand(Guid DcatCatalogId, DcatCatalogRecordInputModel InputModel) : IRequest<Guid>
{ }
