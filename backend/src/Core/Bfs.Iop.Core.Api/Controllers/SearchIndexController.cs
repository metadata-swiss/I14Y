using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

/// <summary>
/// Forwards index maintenance to the IndexSearch service, which receives the caller's token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SearchIndexController : ControllerBase
{
    private readonly IIndexSearchApiClient _indexSearchApiClient;
    private readonly IUserContextService _userContextService;

    public SearchIndexController(
        IIndexSearchApiClient indexSearchApiClient,
        IUserContextService userContextService)
    {
        _indexSearchApiClient = indexSearchApiClient;
        _userContextService = userContextService;
    }

    /// <summary>
    /// Starts a full reindex and returns at once; follow it with the status route.
    /// </summary>
    /// <exception cref="ForbiddenException"></exception>
    /// <exception cref="ConflictException"></exception>
    [HttpPost]
    [Route("reindex")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [Conflict]
    [InternalServerError]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Reindex(CancellationToken cancellationToken = default)
    {
        EnsureUserIsInteroperabilityService();

        try
        {
            var response = await _indexSearchApiClient.PostIndexReindexAsync(cancellationToken);

            return StatusCode(StatusCodes.Status202Accepted, response.Result);
        }
        catch (ApiException exception) when (exception.StatusCode == StatusCodes.Status409Conflict)
        {
            throw new ConflictException("A reindex or another index operation is already running.");
        }
    }

    /// <summary>
    /// Says whether a reindex is running and how the last one ended.
    /// </summary>
    /// <exception cref="ForbiddenException"></exception>
    [HttpGet]
    [Route("status")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IndexStatusResponse))]
    public async Task<IndexStatusResponse> GetStatus(CancellationToken cancellationToken = default)
    {
        EnsureUserIsInteroperabilityService();

        return (await _indexSearchApiClient.GetIndexStatusAsync(cancellationToken)).Result;
    }

    private void EnsureUserIsInteroperabilityService()
    {
        if (_userContextService.GetUserBusinessRole() is not BusinessRole.InteroperabilityService)
        {
            throw new ForbiddenException("The user doesn't have the necessary rights.");
        }
    }
}
