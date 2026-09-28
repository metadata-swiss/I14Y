using Bfs.Iop.AuditTrail.Abstractions.Models;
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

    [HttpGet]
    [Route("commits")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [Ok]
    public async Task<IEnumerable<Commit>> GetCommits(
        [FromQuery] CommitSearchFilters filters, 
        CancellationToken cancellationToken)
    {
        if (_userContextService.GetUserBusinessRole() is not BusinessRole.InteroperabilityService)
        {
            throw new ForbiddenException("The user doesn't have the necessary rights.");
        }

        return await _auditTrailApiClient.GetCommitsAsync(filters, cancellationToken);
    }
}
