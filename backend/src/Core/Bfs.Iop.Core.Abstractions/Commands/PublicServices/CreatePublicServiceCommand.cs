using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record CreatePublicServiceCommand(PublicServiceInputModel InputModel) : IRequest<Guid>
{ }
