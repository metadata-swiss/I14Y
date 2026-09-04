using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Bfs.Iop.Iri.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Iri.Api.Controllers;

[ApiController]
[Route("agent")]
public sealed class AgentsController : ControllerBase
{
    private static readonly string[] AcceptedContentTypes = ["text/html", "*/*", "application/json"];

    private readonly IIopCoreApiClient _apiClient;
    private readonly I14YOptions _i14yOptions;

    public AgentsController(IIopCoreApiClient apiClient, IOptions<I14YOptions> opt)
    {
        _apiClient = apiClient;
        _i14yOptions = opt.Value;
    }

    [HttpGet("{identifier}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> GetAgent([FromRoute] string identifier, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
            return this.UnsupportedMediaType();

        var resp = await _apiClient.GetAgentsByIdentifierAndUidAndPageAndPageSizeAsync(identifier, uid: null, page: null, pageSize: null, cancellationToken);
        var agent = resp.Result.SingleOrDefault();

        if (agent is null)
        {
            return AgentNotFound(identifier);
        }

        var lang = Request.NegotiateLanguage(_i14yOptions);
        var target = $"{_i14yOptions.PublicUiUrl.TrimEnd('/')}/{lang}/organisations/{Uri.EscapeDataString(agent.Id.ToString())}";

        return this.RedirectOrResolveJson(target, resolveOnly);
    }

    private ObjectResult AgentNotFound(string identifier)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Agent not found.",
            detail: $"No agent with identifier '{identifier}' exists.");
    }
}
