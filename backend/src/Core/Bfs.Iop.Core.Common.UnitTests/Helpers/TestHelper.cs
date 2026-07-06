using Bfs.Iop.Infrastructure.Security.Services;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.Core.Common.UnitTests.Helpers;

internal static class TestHelper
{
    public static IUserContextService CreateFakeUserContextService(IEnumerable<Claim> userClaims)
    {
        var user = Substitute.For<ClaimsPrincipal>();
        var context = Substitute.For<IAuthorizationProvider>();

        user.Identity!.IsAuthenticated.Returns(true);
        user.Claims.Returns(userClaims);

        context.GetUser().Returns(user);

        return new UserContextService(context);
    }
}
