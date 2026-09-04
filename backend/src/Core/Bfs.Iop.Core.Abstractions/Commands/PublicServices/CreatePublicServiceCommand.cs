using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record CreatePublicServiceCommand(PublicServiceInputModel InputModel) : IRequest<Guid>
{ }
