using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Bfs.Iop.Iri.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Iri.Api.Controllers;

[ApiController]
[Route("dataset")]
public sealed class DatasetsController : ControllerBase
{
    private static readonly string[] AcceptedContentTypes = ["text/html", "*/*", "application/json"];

    private readonly IIopCoreApiClient _apiClient;
    private readonly I14YOptions _i14yOptions;

    public DatasetsController(IIopCoreApiClient apiClient, IOptions<I14YOptions> opt)
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
    public async Task<IActionResult> GetDataset([FromRoute] string identifier, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
            return this.UnsupportedMediaType();

        try
        {
            var target = await BuildDatasetTarget(identifier, cancellationToken);

            return this.RedirectOrResolveJson(target, resolveOnly);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            //Instead of handling the error here, we forward from iop-core
            return MapDownstreamError(ex);
        }
    }

    [HttpGet("{identifier}/structure/{classId}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetDatasetStructureClass(
        [FromRoute] string identifier,
        [FromRoute] string classId,
        [FromQuery] bool resolveOnly,
        CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
            return this.UnsupportedMediaType();

        try
        {
            var target = await BuildDatasetStructureTarget(
                identifier,
                classId,
                propertyId: null,
                cancellationToken);

            return this.RedirectOrResolveJson(target, resolveOnly);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            //Instead of handling the error here, we forward from iop-core
            return MapDownstreamError(ex);
        }
    }

    [HttpGet("{identifier}/structure/{classId}/{propertyId}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetDatasetStructureProperty(
        [FromRoute] string identifier,
        [FromRoute] string classId,
        [FromRoute] string propertyId,
        [FromQuery] bool resolveOnly,
        CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
            return this.UnsupportedMediaType();

        try
        {
            var target = await BuildDatasetStructureTarget(
                identifier,
                classId,
                propertyId,
                cancellationToken);

            return this.RedirectOrResolveJson(target, resolveOnly);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            //Instead of handling the error here, we forward from iop-core
            return MapDownstreamError(ex);
        }
    }

    private async Task<string> BuildDatasetTarget(
        string identifier,
        CancellationToken cancellationToken)
    {
        var resp = await _apiClient.GetDatasetsByIdentifierByIdentifierAsync(identifier, cancellationToken);
        var dataset = resp.Result;
        var lang = Request.NegotiateLanguage(_i14yOptions);
        var targetIdentifier = dataset.Identifiers.First();

        return $"{_i14yOptions.PublicUiUrl.TrimEnd('/')}/{lang}/catalog/datasets/{Uri.EscapeDataString(targetIdentifier)}";
    }

    private async Task<string> BuildDatasetStructureTarget(
        string identifier,
        string classId,
        string? propertyId,
        CancellationToken cancellationToken)
    {
        var target = await BuildDatasetTarget(identifier, cancellationToken);
        target += $"/structure/{Uri.EscapeDataString(classId)}";

        if (propertyId is not null)
        {
            target += $"/{Uri.EscapeDataString(propertyId)}";
        }

        return target;
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