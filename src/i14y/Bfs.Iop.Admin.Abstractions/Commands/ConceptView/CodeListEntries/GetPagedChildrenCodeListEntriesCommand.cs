using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using MediatR;
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
