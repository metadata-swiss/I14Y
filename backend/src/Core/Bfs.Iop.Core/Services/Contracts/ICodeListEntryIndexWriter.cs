using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Services.Contracts;

/// <summary>
/// The code-list-entry index operations IOP Core performs.
/// <para>
/// The code-list twin of <see cref="ICatalogIndexWriter"/>, and narrow for the same reason: the
/// engine's <c>ICodeListEntryIndexService</c> also carries <c>EnsureIndexAsync</c> and
/// <c>BuildIndexAsync</c>, which rebuild the whole index from Postgres. Core never does that — the
/// IndexSearch service rebuilds on its own schedule.
/// </para>
/// <para>
/// <c>IndexAsync</c> and <c>UpdateIndexAsync</c> are both present because callers distinguish
/// creating entries from editing them; the receiving side reconciles either way, so the split is a
/// caller-side nicety rather than two different behaviours.
/// </para>
/// </summary>
internal interface ICodeListEntryIndexWriter
{
    Task IndexAsync(IEnumerable<CodeListEntryModel> codeListEntries, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<CodeListEntryModel> codeListEntries, CancellationToken cancellationToken = default);

    Task DeIndexAsync(IEnumerable<Guid> codeListEntriesIds, CancellationToken cancellationToken = default);
}
