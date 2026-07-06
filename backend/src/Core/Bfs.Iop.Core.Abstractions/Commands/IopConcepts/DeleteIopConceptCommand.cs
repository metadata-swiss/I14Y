using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record DeleteIopConceptCommand(Guid Id) : IRequest
{ }
