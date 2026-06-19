using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.Core.Common.Extensions;

public static class UserContextExtensions
{
    public static BusinessRole GetUserBusinessRole(this IUserContextService userContextService)
    {
        ArgumentNullException.ThrowIfNull(userContextService, nameof(userContextService));

        return true switch
        {
            _ when userContextService.UserHasRole(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService) 
                => BusinessRole.InteroperabilityService,
            _ when userContextService.UserHasRole(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward) 
                => BusinessRole.LocalDataSteward,
            _ when userContextService.UserHasRole(IopClaimsHelper.Roles.BusinessRoles.Submitter) 
                => BusinessRole.Submitter,
            _ when userContextService.UserHasRole(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer)
                => BusinessRole.StewardshipOrganisationViewer,
            _ when userContextService.UserHasRole(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward)
                => BusinessRole.SwissDataSteward,
            _ => BusinessRole.Unknown,
        };
    }
}
