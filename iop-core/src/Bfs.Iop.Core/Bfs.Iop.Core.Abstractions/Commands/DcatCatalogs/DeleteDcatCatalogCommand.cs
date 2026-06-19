using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record DeleteDcatCatalogCommand(Guid Id) : IRequest
{ }
