using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntriesSearchCommand(
    Guid ConceptId,
    string Language,
    string? Query,
    List<string> Filters,
    bool AddCodeListEntriesPaths,
    int? Page,
    int? PageSize) : IRequest<PagedResult<CodeListEntrySearchResultEntryModel>>
{ }
