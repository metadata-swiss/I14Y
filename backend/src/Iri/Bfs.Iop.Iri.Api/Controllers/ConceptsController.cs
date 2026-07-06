using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Bfs.Iop.Iri.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Iri.Api.Controllers;

[ApiController]
[Route("concept")]
public sealed class ConceptsController : ControllerBase
{
    private static readonly string[] AcceptedContentTypes = ["text/html", "*/*", "application/json"];

    private readonly IIopCoreApiClient _apiClient;
    private readonly I14YOptions _i14yOptions;

    public ConceptsController(IIopCoreApiClient apiClient, IOptions<I14YOptions> opt)
    {
        _apiClient = apiClient;
        _i14yOptions = opt.Value;
    }

    [HttpGet("{identifier}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> GetLatestConcept([FromRoute] string identifier, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
        {
            return this.UnsupportedMediaType();
        }

        var getConceptResponse = await _apiClient.GetConceptsIdentifierByIdentifierAsync(identifier, cancellationToken);

        var concept = SortLatestByVersion(getConceptResponse.Result);

        if (concept is null)
        {
            return ConceptNotFound(identifier);
        }
        
        var lang = Request.NegotiateLanguage(_i14yOptions);
        var target = BuildPublicConceptUrl(concept, lang);

        return this.RedirectOrResolveJson(target, resolveOnly);
    }

    [HttpGet("{identifier}/version/{version}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> GetConceptByVersion([FromRoute] string identifier, [FromRoute] string version, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
        {
            return this.UnsupportedMediaType();
        }

        if (!version.IsValidVersion())
        {
            return InvalidVersionFormat();
        }

        var getConceptResponse = await _apiClient.GetConceptsIdentifierByIdentifierAsync(identifier, cancellationToken);

        var concept = GetByVersion(getConceptResponse.Result, version);

        if (concept is null)
        {
            return ConceptNotFound(identifier, version);
        }

        var lang = Request.NegotiateLanguage(_i14yOptions);
        var target = BuildPublicConceptUrl(concept, lang);

        return this.RedirectOrResolveJson(target, resolveOnly);
    }

    [HttpGet("{identifier}/{code}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetLatestCode([FromRoute] string identifier, [FromRoute] string code, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
            return this.UnsupportedMediaType();

        var conceptsResponse = await _apiClient.GetConceptsIdentifierByIdentifierAsync(identifier, cancellationToken);

        foreach (var concept in SortByVersionDescending(conceptsResponse.Result))
        {
            try
            {
                _ = await _apiClient.GetConceptsCodelistEntriesByCodeByIdAndCodeAsync(concept.Id, code, cancellationToken);

            }
            catch (ApiException<ProblemDetails> apiException) when (apiException.StatusCode == StatusCodes.Status404NotFound)
            {
                continue;
            }

            var parentsResponse = await _apiClient.GetConceptsCodelistEntriesParentsByCodeByIdAndCodeAsync(concept.Id, code, cancellationToken);

            var hierarchicalPath = string.Join("/", parentsResponse.Result.Select(i => Uri.EscapeDataString(i.Code)));

            var lang = Request.NegotiateLanguage(_i14yOptions);
            var target = BuildPublicCodeUrl(concept, hierarchicalPath, lang);

            return this.RedirectOrResolveJson(target, resolveOnly);

        }

        return ConceptNotFound(identifier);
    }

    [HttpGet("{identifier}/{code}/version/{version}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetConceptCodeByConceptAndVersion([FromRoute] string identifier, [FromRoute] string code, [FromRoute] string version, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
        {
            return this.UnsupportedMediaType();
        }

        if (!version.IsValidVersion())
        {
            return InvalidVersionFormat();
        }

        var getConceptResponse = await _apiClient.GetConceptsIdentifierByIdentifierAsync(identifier, cancellationToken);

        var concept = GetByVersion(getConceptResponse.Result, version);

        if (concept is null)
        {
            return ConceptNotFound(identifier, version);
        }

        try
        {
            _ = await _apiClient.GetConceptsCodelistEntriesByCodeByIdAndCodeAsync(concept.Id, code, cancellationToken);

        }
        catch (ApiException<ProblemDetails> apiException) when (apiException.StatusCode == StatusCodes.Status404NotFound)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Codelist entries not found.",
                detail: $"No codelist entry with concept identifier '{identifier}', concept version '{version}' and codelist code '{code}' exists.");
        }

        var parentsResponse = await _apiClient.GetConceptsCodelistEntriesParentsByCodeByIdAndCodeAsync(concept.Id, code, cancellationToken);

        var hierarchicalPath = string.Join("/", parentsResponse.Result.Select(i => Uri.EscapeDataString(i.Code)));

        var lang = Request.NegotiateLanguage(_i14yOptions);
        var target = BuildPublicCodeUrl(concept, hierarchicalPath, lang);

        return this.RedirectOrResolveJson(target, resolveOnly);
    }


    private string BuildPublicConceptUrl(IopConceptModel concept, string lang)
        => $"{_i14yOptions.PublicUiUrl.TrimEnd('/')}/{lang}/catalog/concepts/{Uri.EscapeDataString(concept.Id.ToString())}";
    private string BuildPublicCodeUrl(IopConceptModel concept, string hierarchicalPath, string lang)
        => $"{_i14yOptions.PublicUiUrl.TrimEnd('/')}/{lang}/catalog/concepts/{Uri.EscapeDataString(concept.Id.ToString())}/content/{hierarchicalPath}";

    private static IopConceptModel? SortLatestByVersion(IEnumerable<IopConceptModel> concepts)
    {
        return concepts
            .Select(concept => new { Concept = concept, IsOkVersion = Version.TryParse(concept.Version, out var parsedVersion), Version = parsedVersion })
            .Where(concept => concept.IsOkVersion)
            .OrderByDescending(concept => concept.Version)
            .Select(concept => concept.Concept)
            .FirstOrDefault();
    }

    private static IopConceptModel? GetByVersion(IEnumerable<IopConceptModel> concepts, string version)
    {
        return concepts
            .Where(c => c.Version == version)
            .SingleOrDefault();
    }

    private static IEnumerable<IopConceptModel> SortByVersionDescending(IEnumerable<IopConceptModel> concepts)
    {
        return concepts
            .Select(concept => new { Concept = concept, IsOkVersion = Version.TryParse(concept.Version, out var parsedVersion), Version = parsedVersion })
            .Where(concept => concept.IsOkVersion)
            .OrderByDescending(concept => concept.Version)
            .Select(concept => concept.Concept);
    }



    private ObjectResult ConceptNotFound(string identifier, string? version = null)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Concept not found.",
            detail: string.IsNullOrWhiteSpace(version)
                ? $"No concept with identifier '{identifier}' exists."
                : $"No concept with identifier '{identifier}' and version '{version}' exists.");
    }

    private ObjectResult InvalidVersionFormat()
    {
        return Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Invalid version format.",
            detail: "The 'version' parameter must be a valid semantic version (e.g. 1.2.3).");
    }
}