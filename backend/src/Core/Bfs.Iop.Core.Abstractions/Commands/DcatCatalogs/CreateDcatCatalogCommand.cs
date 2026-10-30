using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record CreateDcatCatalogCommand(DcatCatalogInputModel InputModel) : IRequest<Guid>
{ }
