using Bfs.Iop.DataAccess.Abstractions;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;

public sealed record GetCodeListEntriesForAutoCompleteCommand(
    Guid ConceptId,
    string CodePrefix,
    CodeListEntrySortProperty? SortProperty,
    SortOrder SortOrder,
    int? Page,
    int? PageSize) : IRequest<PagedResult<Models.CodelistEntryInput>>
{ }
