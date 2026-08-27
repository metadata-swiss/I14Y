namespace Bfs.Iop.Core.Data.Indexing;

/// <summary>
/// Answers "may the caller read this concept?" and throws if not.
/// <para>
/// Code-list search is scoped to one concept, and the entries it returns inherit that concept's
/// visibility — so the concept has to be access-checked before the search runs, or an unauthorised
/// caller could enumerate the code list of an internal concept by querying it directly.
/// </para>
/// <para>
/// Extracted from <c>IopConceptsService.GetIopConcept</c>, whose only role on this path was to throw.
/// It lives here rather than in the business layer so the search service can perform the check
/// without referencing <c>Bfs.Iop.Core</c> — and, critically, so that removing that reference could
/// not quietly remove the check with it.
/// </para>
/// </summary>
public interface IConceptAccessGuard
{
    /// <summary>
    /// Throws when the caller may not read the concept.
    /// </summary>
    /// <exception cref="Bfs.Iop.Core.Common.Exceptions.NotFoundException">No such concept.</exception>
    /// <exception cref="Bfs.Iop.Core.Common.Exceptions.ForbiddenException">
    /// The concept exists and the caller is authenticated, but may not read it.
    /// </exception>
    /// <exception cref="Bfs.Iop.Core.Common.Exceptions.UnauthorizedException">
    /// The concept exists and the caller presented no valid token.
    /// </exception>
    Task EnsureUserCanReadConceptAsync(Guid conceptId, CancellationToken cancellationToken = default);
}
