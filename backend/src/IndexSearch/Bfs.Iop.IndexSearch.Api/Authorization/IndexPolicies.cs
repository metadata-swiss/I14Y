using Bfs.Iop.Infrastructure.Security.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Bfs.Iop.IndexSearch.Api.Authorization;

public static class IndexPolicies
{
    public const string Rebuild = "IndexSearch.Rebuild";

    public static void ConfigureRebuild(AuthorizationPolicyBuilder policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        policy
            .RequireAuthenticatedUser()
            .RequireClaim(
                IopClaimsHelper.ClaimTypes.RoleClaimType,
                IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService);
    }
}
