using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntriesHasParentCodesCommand(Guid ConceptId) : IRequest<bool>
{ }
