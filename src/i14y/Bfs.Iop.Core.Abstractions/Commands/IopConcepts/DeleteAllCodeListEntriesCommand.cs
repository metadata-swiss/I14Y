using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record DeleteAllCodeListEntriesCommand(Guid ConceptId) : IRequest
{ }
