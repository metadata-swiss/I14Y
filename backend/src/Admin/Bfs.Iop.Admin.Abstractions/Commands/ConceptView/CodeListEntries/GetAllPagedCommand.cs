using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Cerise;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;

public sealed record GetAllPagedCommand(
    Guid ConceptId,
    int? Page,
    int? PageSize) : IRequest<PagedResult<CodeListEntryDetail>>
{ }