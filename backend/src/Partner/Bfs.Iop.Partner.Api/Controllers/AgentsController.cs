using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Partner.Business.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Bfs.Iop.Partner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AgentsController : ControllerBase
{
    private static readonly string[] _rdfMediaTypes =
        ["text/turtle", "application/x-turtle", "application/rdf+xml"];

    private readonly IIopCoreApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _coreApiBaseUrl;

    public AgentsController(IIopCoreApiClient apiClient, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _coreApiBaseUrl = configuration.GetValue<string>("DcatUrl");
    }

    /// <summary>
    /// Gets the agents matching the given filters.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="uid"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [Produces("application/json", "text/turtle", "application/x-turtle", "application/rdf+xml")]
    [Ok(typeof(DataWrapper<ICollection<AgentModel>>))]
    public async Task<IActionResult> GetAgents(
        string? identifier,
        string? uid,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        if (GetRequestedRdfMediaType() is { } rdfMediaType)
        {
            return await ProxyRdfFromCoreAsync($"api/agents{Request.QueryString}", rdfMediaType, cancellationToken);
        }

        var response = await _apiClient.GetAgentsByIdentifierAndUidAndPageAndPageSizeAsync(identifier, uid, page, pageSize, cancellationToken);

        var pageHeaderValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageHeaderKey);
        var pageSizeValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageSizeHeaderKey);
        var totalPagesValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalPagesHeaderKey);
        var totalRowsValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.Response.Headers.Append(HttpContextExtensions.PageHeaderKey, pageHeaderValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.PageSizeHeaderKey, pageSizeValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalPagesHeaderKey, totalPagesValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalRowsHeaderKey, totalRowsValue);

        return Ok(response.Result.Wrap());
    }

    /// <summary>
    /// Gets the agent with the given id.
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{agentId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Produces("application/json", "text/turtle", "application/x-turtle", "application/rdf+xml")]
    [Ok(typeof(DataWrapper<AgentModel>))]
    public async Task<IActionResult> GetAgent(Guid agentId, CancellationToken cancellationToken)
    {
        if (GetRequestedRdfMediaType() is { } rdfMediaType)
        {
            return await ProxyRdfFromCoreAsync($"api/agents/{agentId}", rdfMediaType, cancellationToken);
        }

        var response = await _apiClient.GetAgentsByIdAsync(agentId, cancellationToken);

        return Ok(response.Result.Wrap());
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
    /// The Partner API stays a pure proxy and does not itself serialize RDF.
    /// </summary>
    private async Task<IActionResult> ProxyRdfFromCoreAsync(string relativePath, string mediaType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_coreApiBaseUrl))
        {
            return StatusCode(StatusCodes.Status502BadGateway, "The Core API base URL (DcatUrl) is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_coreApiBaseUrl.TrimEnd('/')}/{relativePath}");
        request.Headers.Accept.ParseAdd(mediaType);

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return new ContentResult
        {
            Content = content,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? mediaType,
            StatusCode = (int)response.StatusCode,
        };
    }
}
