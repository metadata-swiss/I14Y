using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record DeleteCodeListEntryCommand(Guid ConceptId, Guid CodeListEntryId) : IRequest
{ }
