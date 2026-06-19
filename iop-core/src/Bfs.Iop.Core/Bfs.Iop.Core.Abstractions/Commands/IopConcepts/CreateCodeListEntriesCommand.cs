using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record CreateCodeListEntriesCommand(
    Guid ConceptId, 
    IEnumerable<CodeListEntryInputModel> InputModels) : IRequest<IEnumerable<Guid>>
{ }
