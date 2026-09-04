using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Bfs.Iop.Iri.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Iri.Api.Controllers;

[ApiController]
[Route("dataservice")]
public sealed class DataservicesController : ControllerBase
{
    private static readonly string[] AcceptedContentTypes = ["text/html", "*/*", "application/json"];

    private readonly IIopCoreApiClient _apiClient;
    private readonly I14YOptions _i14yOptions;

    public DataservicesController(IIopCoreApiClient apiClient, IOptions<I14YOptions> opt)
    {
        _apiClient = apiClient;
        _i14yOptions = opt.Value;
    }

    [HttpGet("{identifier}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetDataservice([FromRoute] string identifier, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
            return this.UnsupportedMediaType();

        try
        {
            var resp = await _apiClient.GetDataServicesByIdentifierByIdentifierAsync(identifier, cancellationToken);
            var dataservice = resp.Result;
            var lang = Request.NegotiateLanguage(_i14yOptions);
            var targetIdentifier = dataservice.Identifiers.First();
            var target = $"{_i14yOptions.PublicUiUrl.TrimEnd('/')}/{lang}/catalog/dataservices/{Uri.EscapeDataString(targetIdentifier)}";

            return this.RedirectOrResolveJson(target, resolveOnly);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            //Instead of handling the error here, we forward from iop-core
            return MapDownstreamError(ex);
        }
    }

    private IActionResult MapDownstreamError(ApiException<ProblemDetails> ex)
    {
        if (ex.Result is not null)
        {
            return Problem(
                statusCode: ex.StatusCode,
                title: ex.Result.Title,
                detail: ex.Result.Detail,
                type: ex.Result.Type,
                instance: ex.Result.Instance);
        }

        return Problem(
            statusCode: ex.StatusCode,
            title: "Downstream API error.",
            detail: ex.Message);
    }
}