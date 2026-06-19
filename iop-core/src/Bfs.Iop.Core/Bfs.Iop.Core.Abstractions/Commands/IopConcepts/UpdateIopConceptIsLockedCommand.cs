using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record UpdateIopConceptIsLockedCommand(Guid Id, bool Value) : IRequest
{ }
