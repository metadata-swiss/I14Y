using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record GetDcatCatalogCommand(Guid Id) : IRequest<DcatCatalogModel>
{ }
