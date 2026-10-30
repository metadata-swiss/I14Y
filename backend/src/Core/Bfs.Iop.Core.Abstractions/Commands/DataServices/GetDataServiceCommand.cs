using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DataServices;

public sealed record GetDataServiceCommand(Guid DataServiceId) : IRequest<DataServiceModel>
{ }
