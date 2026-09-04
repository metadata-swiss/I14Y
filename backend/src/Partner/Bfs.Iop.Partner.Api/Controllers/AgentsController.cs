using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Partner.Business.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Bfs.Iop.Partner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AgentsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public AgentsController(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

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
    [Ok(typeof(DataWrapper<ICollection<AgentModel>>))]
    public async Task<DataWrapper<ICollection<AgentModel>>> GetAgents(
        string? identifier,
        string? uid,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAgentsByIdentifierAndUidAndPageAndPageSizeAsync(identifier, uid, page, pageSize, cancellationToken);

        var pageHeaderValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageHeaderKey);
        var pageSizeValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageSizeHeaderKey);
        var totalPagesValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalPagesHeaderKey);
        var totalRowsValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.Response.Headers.Append(HttpContextExtensions.PageHeaderKey, pageHeaderValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.PageSizeHeaderKey, pageSizeValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalPagesHeaderKey, totalPagesValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalRowsHeaderKey, totalRowsValue);

        return response.Result.Wrap();
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
    [Ok(typeof(DataWrapper<AgentModel>))]
    public async Task<DataWrapper<AgentModel>> GetAgent(Guid agentId, CancellationToken cancellationToken) =>
        (await _apiClient.GetAgentsByIdAsync(agentId, cancellationToken)).Result.Wrap();

    /// <summary>
    /// Exports all agents (organisations) in one of the standard RDF formats.
    /// </summary>
    /// <param name="dataFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("exports/{dataFormat}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [Produces("application/rdf+xml", "application/x-turtle")]
    [BadRequest]
    [InternalServerError]
    [Ok(typeof(string))]
    public async Task<IActionResult> ExportAgents(
        RdfExportFormat dataFormat,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetAgentsExportByDataFormatAsync(dataFormat, cancellationToken);

        var content = response.Result;
        var contentType = response.Headers[HeaderNames.ContentType].Single();

        return Content(content, contentType);
    }
}
