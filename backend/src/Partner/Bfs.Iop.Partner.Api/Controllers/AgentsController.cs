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
    /// Returns the RDF media type requested via the Accept header (Turtle or RDF/XML), or null when JSON is
    /// preferred (higher or equal quality) or no RDF type is acceptable.
    /// </summary>
    private string? GetRequestedRdfMediaType()
    {
        var accept = Request.GetTypedHeaders().Accept;
        if (accept is null || accept.Count == 0)
        {
            return null;
        }

        var best = accept
            .Where(x => (x.Quality ?? 1.0) > 0 && x.MediaType.HasValue)
            .Select(x => new { MediaType = x.MediaType.Value!, Quality = x.Quality ?? 1.0 })
            .Where(x =>
                _rdfMediaTypes.Contains(x.MediaType, StringComparer.OrdinalIgnoreCase)
                || string.Equals(x.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Quality)
            // Prefer JSON when qualities are equal to keep backward compatible behavior.
            .ThenBy(x => string.Equals(x.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .FirstOrDefault();

        return best is null || string.Equals(best.MediaType, "application/json", StringComparison.OrdinalIgnoreCase)
            ? null
            : best.MediaType;
    }

    /// <summary>
    /// Forwards the request to the Core API (which produces the RDF) and streams back its response body / status code.
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
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        HttpContext.Response.RegisterForDispose(response);

        HttpContext.Response.StatusCode = (int)response.StatusCode;
        var contentType = response.Content.Headers.ContentType?.ToString() ?? mediaType;
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return File(stream, contentType);
    }
}
