using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator) => 
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="uid"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Produces("application/json", "text/turtle", "application/x-turtle", "application/rdf+xml")]
    [Ok(typeof(IEnumerable<AgentModel>))]
    public async Task<IEnumerable<AgentModel>> GetAgents(
        string? identifier,
        string? uid,
        int? page, 
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAgentsCommand(identifier, uid, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Produces("application/json", "text/turtle", "application/x-turtle", "application/rdf+xml")]
    [Ok(typeof(AgentModel))]
    public Task<AgentModel> GetAgent(Guid id, CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetAgentCommand(id), cancellationToken);

    /// <summary>
    /// Gets the agents published statistics about the resources that the current user is allowed to access.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("statistics")]
    [AllowAnonymous]
    [InternalServerError]
    [Ok(typeof(IEnumerable<AgentStatisticsResult>))]
    public Task<IEnumerable<AgentStatisticsResult>> GetAgentsStatistics(CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetAgentsStatisticsCommand(), cancellationToken);

    /// <summary>
    /// Returns the parent agents from one specific agent.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/sub-agent-of")]
    [AllowAnonymous]
    [BadRequest]
    [InternalServerError]
    [Ok(typeof(IEnumerable<AgentModel>))]
    public Task<IEnumerable<AgentModel>> GetAgentParentAgents(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetAgentParentAgentsCommand(id), cancellationToken);

    [HttpPost]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostAgent(AgentInputModel inputModel, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateAgentCommand(inputModel), cancellationToken);

        return CreatedAtAction(nameof(GetAgent), new { id = result }, result);
    }

    [HttpPut]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutAgent(Guid id, AgentInputModel updateModel, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateAgentCommand(id, updateModel), cancellationToken);

        return NoContent();
    }

    [HttpDelete]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteAgent(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAgentCommand(id), cancellationToken);

        return NoContent();
    }
}
