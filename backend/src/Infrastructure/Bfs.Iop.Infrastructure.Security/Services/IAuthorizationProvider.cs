using System.Security.Claims;

namespace Bfs.Iop.Infrastructure.Security.Services;

public interface IAuthorizationProvider
{
    ClaimsPrincipal GetUser();
}
