using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuditTrailController : ControllerBase
{
    private readonly IAuditTrailApiClient _auditTrailApiClient;
    private readonly IUserContextService _userContextService;

    public AuditTrailController(
        IAuditTrailApiClient auditTrailApiClient, 
        IUserContextService userContextService)
    {
        _auditTrailApiClient = auditTrailApiClient;
        _userContextService = userContextService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ForbiddenException"></exception>
    [HttpGet]
    [Route("commits")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IEnumerable<Commit>))]
    public async Task<IEnumerable<Commit>> GetCommits(
        [FromQuery] CommitSearchFilters filters,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        if (_userContextService.GetUserBusinessRole() is not BusinessRole.InteroperabilityService)
        {
            throw new ForbiddenException("The user doesn't have the necessary rights.");
        }

        var pagedResult = await _auditTrailApiClient.GetCommitsAsync(filters, page, pageSize, cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }
}
