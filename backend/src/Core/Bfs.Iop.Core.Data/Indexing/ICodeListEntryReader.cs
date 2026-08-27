using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Indexing;

/// <summary>
/// Hydrates code-list entries for search results.
/// <para>
/// Elasticsearch returns ids and scores; the rows themselves still come from Postgres, so a search
/// result carries the same data any other read would return. These reads are the query-time
/// counterpart of <see cref="IIndexDataReader"/>, which feeds the index.
/// </para>
/// <para>
/// <b>Deliberately unauthorized</b>, matching the methods on <c>IIopConceptsService</c> they replace:
/// neither applied a user predicate. Access control for this path is enforced once, up front, by
/// <see cref="IConceptAccessGuard"/> on the owning concept — entries inherit its visibility. That
/// makes the guard load-bearing: drop it and these become an unauthenticated read of any code list.
/// </para>
/// </summary>
public interface ICodeListEntryReader
{
    /// <summary>
    /// Returns entries in the order the ids were given. Ids with no row are skipped, because
    /// Elasticsearch can return an id whose row has since been deleted.
    /// </summary>
    Task<IReadOnlyList<CodeListEntryModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one concept's entries by code, in the order the codes were given.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// A requested code does not exist on the concept. Preserved from the original: this path walks
    /// parent codes, and a dangling parent reference is a data fault worth surfacing rather than
    /// silently truncating the ancestor path.
    /// </exception>
    Task<IReadOnlyList<CodeListEntryModel>> GetByCodesAsync(Guid conceptId, IEnumerable<string> codes, CancellationToken cancellationToken = default);
}
