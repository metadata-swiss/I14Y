using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.Core.Extensions;

internal static class UserContextServiceExtensions
{
    public static void EnsureUserIsAllowed(this IUserContextService userContextService, IEnumerable<BusinessRole> allowedBusinessRoles)
    {
        ArgumentNullException.ThrowIfNull(userContextService, nameof(userContextService));
        ArgumentNullException.ThrowIfNull(allowedBusinessRoles, nameof(allowedBusinessRoles));

        var businessRole = userContextService.GetUserBusinessRole();

        if (!allowedBusinessRoles.Contains(businessRole))
        {
            throw userContextService.IsUserTokenValid()
                ? new ForbiddenException("The user has not enough rights to perform the action.")
                : new UnauthorizedException("The user has no valid token.");
        }
    }
}
