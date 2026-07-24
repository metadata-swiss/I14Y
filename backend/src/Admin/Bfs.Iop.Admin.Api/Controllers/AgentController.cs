using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MapsterMapper;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The agent controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private static readonly string[] _rdfMediaTypes =
        ["text/turtle", "application/x-turtle", "application/rdf+xml"];

    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;
    private readonly string? _coreApiBaseUrl;

    /// <summary>
    /// Initializes a <see cref="AgentController"/> instance
    /// </summary>
    /// <param name="apiClient"></param>
    /// <param name="mapper"></param>
    /// <param name="httpClient"></param>
    /// <param name="configuration"></param>
    public AgentController(IIopCoreApiClient apiClient, IMapper mapper, HttpClient httpClient, IConfiguration configuration)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _httpClient = httpClient;
        _coreApiBaseUrl = configuration.GetValue<string>("DcatUrl");
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
    [Produces("application/json", "text/turtle", "application/x-turtle", "application/rdf+xml")]
    [AllowAnonymous]
    [Ok(typeof(Agent))]
    public async Task<IActionResult> GetAgent(Guid id, CancellationToken cancellationToken)
    {
        if (GetRequestedRdfMediaType() is { } rdfMediaType)
        {
            return await ProxyRdfFromCoreAsync($"api/agents/{id}", rdfMediaType, cancellationToken);
        }

        var response = await _apiClient.GetAgentsByIdAsync(id, cancellationToken);

        var agent = _mapper.Map<Agent>(response.Result);

        var parentAgentsResponse = await _apiClient.GetAgentsSubAgentOfByIdAsync(agent.Id, cancellationToken);

        agent.SubAgentOf = _mapper.Map<IEnumerable<IdNameModel>>(parentAgentsResponse.Result);

        return Ok(agent);
    }

    /// <summary>
    /// Lists all agents
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Produces("application/json", "text/turtle", "application/x-turtle", "application/rdf+xml")]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<Agent>))]
    public async Task<IActionResult> GetAllAgents(CancellationToken cancellationToken)
    {
        if (GetRequestedRdfMediaType() is { } rdfMediaType)
        {
            return await ProxyRdfFromCoreAsync("api/agents", rdfMediaType, cancellationToken);
        }

        var response = await _apiClient.GetAgentsByIdentifierAndUidAndPageAndPageSizeAsync(
            identifier: null,
            uid: null,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken: cancellationToken);

        var agents = _mapper.Map<IEnumerable<Agent>>(response.Result);

        await GetAndFillSubAgentOf(agents, cancellationToken);

        return Ok(agents);
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

        await GetAndFillSubAgentOf(agents, cancellationToken);

        _ = await _apiClient.GetPersonsSelfRegisteredAsync(cancellationToken);

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

    /// <summary>
    /// Returns the RDF media type requested via the Accept header (Turtle or RDF/XML), or null when JSON is requested.
    /// </summary>
    private string? GetRequestedRdfMediaType() =>
        Request.GetTypedHeaders().Accept?
            .Where(x => (x.Quality ?? 1.0) > 0)
            .OrderByDescending(x => x.Quality ?? 1.0)
            .Select(x => x.MediaType.Value)
            .FirstOrDefault(x => x is not null && _rdfMediaTypes.Contains(x, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Forwards the request to the Core API (which produces the RDF) and returns its response verbatim.
    /// The Admin API stays a pure proxy and does not itself serialize RDF.
    /// </summary>
    private async Task<IActionResult> ProxyRdfFromCoreAsync(string relativePath, string mediaType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_coreApiBaseUrl))
        {
            return StatusCode(StatusCodes.Status502BadGateway, "The Core API base URL (DcatUrl) is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_coreApiBaseUrl.TrimEnd('/')}/{relativePath}");
        request.Headers.Accept.ParseAdd(mediaType);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return new ContentResult
        {
            Content = content,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? mediaType,
            StatusCode = (int)response.StatusCode,
        };
    }

    private Task GetAndFillSubAgentOf(IEnumerable<Agent> agents, CancellationToken cancellationToken) =>
        Parallel.ForEachAsync(
             agents,
             new ParallelOptions
             {
                 MaxDegreeOfParallelism = 10,
                 CancellationToken = cancellationToken
             },
             async (agent, ct) =>
             {
                 var parentAgentsResponse =
                    await _apiClient.GetAgentsSubAgentOfByIdAsync(agent.Id, ct);

                 agent.SubAgentOf =
                     _mapper.Map<IEnumerable<IdNameModel>>(parentAgentsResponse.Result);
             });
}