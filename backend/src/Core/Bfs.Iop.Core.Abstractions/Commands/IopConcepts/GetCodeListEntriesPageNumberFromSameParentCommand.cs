using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntriesPageNumberFromSameParentCommand(
    Guid ConceptId,
    string Code,
    CodeListEntrySortProperty? SortProperty,
    SortOrder SortOrder,
    int? PageSize) : IRequest<int>
{ }
