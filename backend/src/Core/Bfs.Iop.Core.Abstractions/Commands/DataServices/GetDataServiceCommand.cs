using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DataServices;

public sealed record GetDataServiceCommand(Guid DataServiceId) : IRequest<DataServiceModel>
{ }
