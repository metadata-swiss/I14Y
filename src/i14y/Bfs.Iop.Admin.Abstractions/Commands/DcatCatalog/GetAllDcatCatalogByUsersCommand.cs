using MediatR;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.DcatCatalog;

public class GetAllDcatCatalogsByUserCommand : IRequest<IEnumerable<Models.DcatCatalog>>
{
}