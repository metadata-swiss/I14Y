using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The Users controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IIopCoreApiClient _dcatClient;

    public UsersController(IIopCoreApiClient dcatClient) => 
        _dcatClient = dcatClient ?? throw new ArgumentNullException(nameof(dcatClient));

    /// <summary>
    /// Retrieves the information about the current logged user.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("user-info")]
    [AllowAnonymous]
    [Ok(typeof(UserModel))]
    public async Task<UserModel> GetCurrentUserInfo(CancellationToken cancellationToken) => 
        (await _dcatClient.GetUsersCurrentAsync(cancellationToken)).Result;
}
