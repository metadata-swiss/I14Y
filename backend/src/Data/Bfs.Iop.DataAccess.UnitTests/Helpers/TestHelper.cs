using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.DataAccess.UnitTests.Helpers;

internal static class TestHelper
{
    public static IUserContextService CreateFakeUserContextService(IEnumerable<Claim> claims)
    {
        var provider = Substitute.For<IAuthorizationProvider>();
        var user = Substitute.For<ClaimsPrincipal>();
        user.Identity!.IsAuthenticated.Returns(true);
        user.Claims.Returns(claims);

        provider.GetUser().Returns(user);

        return new UserContextService(provider);
    }

    public static IUserContextService CreateFakeUserContextServiceForInteroperabilityServiceUser()
    {
        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, "*")
        };

        return CreateFakeUserContextService(claims);
    }
}
