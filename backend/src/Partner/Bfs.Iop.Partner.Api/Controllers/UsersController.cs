using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Partner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public UsersController(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Returns the current user information.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("current")]
    [AllowAnonymous]
    [Ok(typeof(UserModel))]
    public async Task<UserModel> GetCurrentUser(CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetUsersCurrentAsync(cancellationToken);
        return response.Result;
    }
}
