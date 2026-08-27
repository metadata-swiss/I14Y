using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Data.Indexing;

/// <inheritdoc cref="ICodeListEntryReader"/>
/// <remarks>
/// The <c>Include</c> set matches the queries this replaces. Both are load-bearing for a search
/// result: <c>Annotations</c> ordered by position is what the UI renders, and
/// <c>ParentCodeListEntry</c> is what <c>MapToCodeListEntryModel</c> dereferences to fill
/// <c>ParentCode</c> — without it every entry looks like a root and the ancestor path collapses.
/// </remarks>
internal sealed class CodeListEntryReader : ICodeListEntryReader
{
    private readonly IopDbContext _dbContext;

    public CodeListEntryReader(IopDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<CodeListEntryModel>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var wanted = ids.ToArray();
        if (wanted.Length == 0)
        {
            return [];
        }

        var byId = await Query()
            .Where(x => wanted.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // Order follows the ids, not the database: those ids are Elasticsearch's relevance ordering,
        // and re-sorting here would silently discard the ranking.
        return [.. wanted.Where(byId.ContainsKey).Select(id => byId[id].MapToCodeListEntryModel())];
    }

    public async Task<IReadOnlyList<CodeListEntryModel>> GetByCodesAsync(
        Guid conceptId,
        IEnumerable<string> codes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(codes);

        var wanted = codes.ToArray();
        if (wanted.Length == 0)
        {
            return [];
        }

        var byCode = await Query()
            .Where(x => x.IopConceptId == conceptId && wanted.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, cancellationToken);

        return [.. wanted.Select(code => byCode[code].MapToCodeListEntryModel())];
    }

    private IQueryable<CodeListEntry> Query() =>
        _dbContext.CodeListEntries
            .AsNoTracking()
            .Include(c => c.Annotations.OrderBy(a => a.Position))
            .Include(c => c.ParentCodeListEntry);
}
