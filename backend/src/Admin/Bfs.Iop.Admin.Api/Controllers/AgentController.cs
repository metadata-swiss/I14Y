using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The agent controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a <see cref="AgentController"/> instance
    /// </summary>
    /// <param name="apiClient"></param>
    /// <param name="mapper"></param>
    public AgentController(IIopCoreApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets the Agent with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(Agent))]
    public async Task<Agent> GetAgent(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAgentsByIdAsync(id, cancellationToken);

        var agent = _mapper.Map<Agent>(response.Result);

        var parentAgentsResponse = await _apiClient.GetAgentsSubAgentOfByIdAsync(agent.Id, cancellationToken);

        agent.SubAgentOf = _mapper.Map<IEnumerable<IdNameModel>>(parentAgentsResponse.Result);

        return agent;
    }

    /// <summary>
    /// Lists all agents
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<Agent>))]
    public async Task<IEnumerable<Agent>> GetAllAgents(CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAgentsByIdentifierAndUidAndPageAndPageSizeAsync(
            identifier: null,
            uid: null,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken: cancellationToken);

        var agents = _mapper.Map<IEnumerable<Agent>>(response.Result);

        return agents;
    }

    /// <summary>
    /// Lists all agents to which the current user has access to
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("user")]
    [Authorize()]
    [ProducesJson]
    [Unauthorized]
    [NotFound]
    [Ok(typeof(IEnumerable<Agent>))]
    public async Task<IEnumerable<Agent>> GetUserAgents(CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetUsersCurrentAgentsAsync(cancellationToken);

        var agents = _mapper.Map<IEnumerable<Agent>>(response.Result);

        return agents;
    }

    /// <summary>
    /// Gets the agents published statistics about the resources that the current user is allowed to access.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("statistics")]
    [AllowAnonymous]
    [InternalServerError]
    [Ok(typeof(IEnumerable<AgentStatisticsResult>))]
    public async Task<IEnumerable<AgentStatisticsResult>> GetAgentsStatistics(CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetAgentsStatisticsAsync(cancellationToken);

        return response.Result;
    }
}