using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DataServices;

public sealed record DeleteDataServiceCommand(Guid Id) : IRequest
{ }
