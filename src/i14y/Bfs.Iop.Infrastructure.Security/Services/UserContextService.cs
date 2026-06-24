using Bfs.Iop.Infrastructure.Security.Configuration;
using Bfs.Iop.Infrastructure.Security.Helpers;

namespace Bfs.Iop.Infrastructure.Security.Services;

internal sealed class UserContextService : IUserContextService
{
    private readonly IAuthorizationProvider _authorizationProvider;

    public UserContextService(IAuthorizationProvider authorizationProvider) => 
        _authorizationProvider = authorizationProvider 
            ?? throw new ArgumentNullException(nameof(authorizationProvider));

    public bool IsUserTokenValid()
    {
        var user = _authorizationProvider.GetUser();

        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            return false;
        }

        // Check if at least one of these roles is present.
        var allowRoles = new string[]
        {
            IopClaimsHelper.Roles.General.Allow,
            IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward,
            IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService,
            IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward,
            IopClaimsHelper.Roles.BusinessRoles.Submitter,
            IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer
        };

        foreach (var role in allowRoles)
        {
            if (UserHasRole(role))
            {
                return true; 
            }
        }

        return false;
    }

    public string? TryGetUserClaimValue(string claimType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claimType, nameof(claimType));

        var user = _authorizationProvider.GetUser();

        return user.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }

    public IReadOnlyList<string> GetUserAgencies()
    {
        if (!IsUserTokenValid())
        {
            return [];
        }

        var claimValue = TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.AgenciesClaimType);

        if (claimValue is null)
        {
            return [];
        }

        var agencies = claimValue.Split(";");
        agencies = [.. agencies.Select(x =>
        {
            var lastIndexOf = x.LastIndexOf('\\');

            return lastIndexOf != -1 
            ? x.Substring(lastIndexOf + 1, x.Length - lastIndexOf - 1) 
            : x;
        })];

        return agencies;
    }

    public bool UserBelongsToAgency(string agencyIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agencyIdentifier, nameof(agencyIdentifier));

        return GetUserAgencies().Any(x => x == "*" || x == agencyIdentifier);
    }

    public bool UserHasRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role, nameof(role));

        var user = _authorizationProvider.GetUser();

        return user.Claims
            .Where(x => x.Type == IopClaimsHelper.ClaimTypes.RoleClaimType)
            .Any(x => x.Value == role);
    }
}
