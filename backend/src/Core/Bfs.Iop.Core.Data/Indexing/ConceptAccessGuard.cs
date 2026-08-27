using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Data.Indexing;

/// <inheritdoc cref="IConceptAccessGuard"/>
/// <remarks>
/// The predicate below is the read-authorization rule from
/// <c>PublishableEntityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery</c>,
/// applied to a single concept. It is deliberately the same rule the Elasticsearch query builder
/// applies to search results, so a concept a user cannot find in search is also a concept whose code
/// list they cannot open directly.
/// <para>
/// If that rule ever changes, it must change in both places. The alternative — sharing the business
/// layer's implementation — is what forced the search service to reference <c>Bfs.Iop.Core</c> in the
/// first place.
/// </para>
/// </remarks>
internal sealed class ConceptAccessGuard : IConceptAccessGuard
{
    private readonly IopDbContext _dbContext;
    private readonly IUserContextService _userContextService;

    public ConceptAccessGuard(IopDbContext dbContext, IUserContextService userContextService)
    {
        _dbContext = dbContext;
        _userContextService = userContextService;
    }

    public async Task EnsureUserCanReadConceptAsync(Guid conceptId, CancellationToken cancellationToken = default)
    {
        var visible = await Authorized(_dbContext.IopConcepts.AsNoTracking())
            .AnyAsync(x => x.Id == conceptId, cancellationToken);

        if (visible)
        {
            return;
        }

        // Distinguish "does not exist" from "exists but is not yours" exactly as the business layer
        // does. Collapsing them would be a small information leak in one direction and a confusing
        // 404-for-a-permissions-problem in the other.
        var exists = await _dbContext.IopConcepts
            .AsNoTracking()
            .AnyAsync(x => x.Id == conceptId, cancellationToken);

        throw !exists
            ? new NotFoundException("No resource has been found.")
            : _userContextService.IsUserTokenValid()
                ? new ForbiddenException("The resource is forbidden.")
                : new UnauthorizedException("No authorization to access the resource.");
    }

    private IQueryable<IopConcept> Authorized(IQueryable<IopConcept> query)
    {
        var role = _userContextService.GetUserBusinessRole();
        var agencies = _userContextService.GetUserAgencies();

        return role switch
        {
            BusinessRole.SwissDataSteward or
            BusinessRole.InteroperabilityService => query,

            BusinessRole.Unknown => query.Where(x => x.PublicationLevel == PublicationLevel.Public),

            BusinessRole.StewardshipOrganisationViewer or
            BusinessRole.LocalDataSteward or
            BusinessRole.Submitter => query.Where(x =>
                x.PublicationLevel == PublicationLevel.Public ||
                (x.PublicationLevel == PublicationLevel.Internal && agencies.Contains(x.Publisher.Identifier))),

            // Fail closed. An unrecognised role must not fall through to "everything is visible".
            _ => throw new NotSupportedException($"The business role '{role}' is not supported."),
        };
    }
}
