using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record DeletePublicServiceCommand(Guid Id) : IRequest
{ }
