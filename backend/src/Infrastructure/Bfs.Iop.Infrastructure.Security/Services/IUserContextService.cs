namespace Bfs.Iop.Infrastructure.Security.Services;

public interface IUserContextService
{
    bool IsUserTokenValid();

    string? TryGetUserClaimValue(string claimType);

    IReadOnlyList<string> GetUserAgencies();

    bool UserBelongsToAgency(string agencyIdentifier);

    bool UserHasRole(string role);

    BusinessRole GetUserBusinessRole();
}
