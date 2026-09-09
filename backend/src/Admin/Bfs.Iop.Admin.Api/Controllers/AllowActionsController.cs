using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The allowActions controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AllowActionsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="AllowActionsController"/> instance
    /// </summary>
    /// <param name="apiClient"></param>
    public AllowActionsController(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient;

    /// <summary>
    /// Returns the possible creation authorizations according to the user business role.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet("allow-create")] // ToDo: Remove the allow-create once the old endpoint is deleted
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<AllowActionResult>))]
    public async Task<IEnumerable<AllowActionResult>> GetAllowCreateInfo(CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAllowActionsAsync(cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Returns the user possible authorizations for a specific resource.
    /// </summary>
    /// <param name="resourceType">Resource type.</param>
    /// <param name="id">Resource id.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{resourceType}/{id:guid}/v2")] // ToDo: Remove the v2 once the old endpoint is deleted
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<AllowActionResult>))]
    [BadRequest]
    public async Task<IEnumerable<AllowActionResult>> GetAllowActionInfo(
        AllowActionResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAllowActionsByResourceTypeAndResourceIdAsync(
            resourceType, 
            id,
            cancellationToken);

        return response.Result;
    }

    /// <summary>
    /// Gets the information if the application is in read only mode.
    /// </summary>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("read-only")]
    [AllowAnonymous]
    [InternalServerError]
    [Ok(typeof(bool))]
    public async Task<bool> IsApplicationReadOnly(CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAllowActionsReadOnlyAsync(cancellationToken);

        return response.Result;
    }
}