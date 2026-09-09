using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntriesChildrenOfParentCodeCommand(
    Guid ConceptId,
    string ParentCode,
    CodeListEntrySortProperty? SortProperty,
    SortOrder SortOrder,
    int? Page,
    int? PageSize) : IRequest<PagedResult<CodeListEntryModel>>
{ }
