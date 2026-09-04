using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record UpdateCodeListEntryCommand(
    Guid ConceptId,
    Guid CodeListEntryId,
    CodeListEntryInputModel UpdateModel) : IRequest
{ }
