using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record CreateCodeListEntriesCommand(
    Guid ConceptId, 
    IEnumerable<CodeListEntryInputModel> InputModels) : IRequest<IEnumerable<Guid>>
{ }
