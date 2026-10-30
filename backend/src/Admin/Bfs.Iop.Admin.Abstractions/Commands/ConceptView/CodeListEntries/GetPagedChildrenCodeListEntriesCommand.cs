using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Cerise;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;

public sealed record GetPagedChildrenCodeListEntriesCommand(
    Guid ConceptId,
    string Code,
    CodeListEntrySortProperty? SortProperty,
    SortOrder SortOrder,
    int? Page,
    int? PageSize) : IRequest<PagedResult<CodeListEntryDetail>>
{ }
