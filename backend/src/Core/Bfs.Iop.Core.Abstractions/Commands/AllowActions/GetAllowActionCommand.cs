using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.AllowActions;

public sealed record GetAllowActionCommand(
    AllowActionResourceType ResourceType,
    Guid ResourceId) : IRequest<IEnumerable<AllowActionResult>>
{ }