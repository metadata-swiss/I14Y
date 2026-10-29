using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DataServices;

public sealed record CreateDataServiceCommand(DataServiceInputModel InputModel) : IRequest<Guid>
{ }
