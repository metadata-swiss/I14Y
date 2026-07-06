using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes; 
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Bfs.Iop.Iri.Api.Extensions;

namespace Bfs.Iop.Iri.Api.Controllers;

[ApiController]
[Route("mappingTable")]
public sealed class MappingTablesController : ControllerBase
{
    private static readonly string[] AcceptedContentTypes = ["text/html", "*/*", "application/json"];

    private readonly IIopCoreApiClient _apiClient; 
    private readonly I14YOptions _i14yOptions;

    public MappingTablesController(IIopCoreApiClient apiClient, IOptions<I14YOptions> opt)
    {
        _apiClient = apiClient; 
        _i14yOptions = opt.Value;
    }

    [HttpGet("{identifier}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> GetLatestMappingTable([FromRoute] string identifier, [FromQuery] bool resolveOnly, CancellationToken cancellationToken)
    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
        {
            return this.UnsupportedMediaType();
        }

        var getMappingResponse = await _apiClient.GetMappingTablesByMappingTableIdentifierAndPublisherIdentifierAndVersionAndCodeSystemUriAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            mappingTableIdentifier: identifier,
            publisherIdentifier: null,
            version: null,
            codeSystemUri:null,
            publicationLevel: null,
            registrationStatus: null,
            page: null,
            pageSize: null,
            cancellationToken);

        var mappingTable = SortLatestByVersion(getMappingResponse.Result);

        if (mappingTable is null)
        {
            return MappingTableNotFound(identifier);
        }

        var lang = Request.NegotiateLanguage(_i14yOptions);
        var target = BuildMappingTableUrl(mappingTable, lang);

        return this.RedirectOrResolveJson(target, resolveOnly);
    }

    [HttpGet("{identifier}/version/{version}")]
    [NotFound]
    [Ok]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetMappingTableByVersion(
        [FromRoute] string identifier,
        [FromRoute] string version,
        [FromQuery] bool resolveOnly,
        CancellationToken cancellationToken)

    {
        if (!Request.AcceptsContentTypes(AcceptedContentTypes))
        {
            return this.UnsupportedMediaType();
        }

        if (!version.IsValidVersion())
        {
            return InvalidVersionFormat();
        }

        var getMappingTablesResponse = await _apiClient.GetMappingTablesByMappingTableIdentifierAndPublisherIdentifierAndVersionAndCodeSystemUriAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            mappingTableIdentifier: identifier,
            publisherIdentifier: null,
            version: version,
            codeSystemUri:null,
            publicationLevel: null,
            registrationStatus: null,
            page: null,
            pageSize: null,
            cancellationToken);

        var mappingTable = GetByVersion(getMappingTablesResponse.Result, version);

        if (mappingTable is null)
        {
            return MappingTableNotFound(identifier, version);
        }

        var lang = Request.NegotiateLanguage(_i14yOptions);
        var target = BuildMappingTableUrl(mappingTable, lang);

        return this.RedirectOrResolveJson(target, resolveOnly);
    }

    private string BuildMappingTableUrl(MappingTableModel mappingTable, string lang)
        => $"{_i14yOptions.PublicUiUrl.TrimEnd('/')}/{lang}/catalog/mappingtables/{Uri.EscapeDataString(mappingTable.Id.ToString())}";

    private static MappingTableModel? SortLatestByVersion(IEnumerable<MappingTableModel> mappingTables)
    {
        return mappingTables
            .Select(mappingTable => new { MappingTable = mappingTable, IsOkVersion = Version.TryParse(mappingTable.Version, out var parsedVersion), Version = parsedVersion })
            .Where(mappingTable => mappingTable.IsOkVersion)
            .OrderByDescending(mappingTable => mappingTable.Version)
            .Select(mappingTable => mappingTable.MappingTable)
            .FirstOrDefault();
    }

    private static MappingTableModel? GetByVersion(IEnumerable<MappingTableModel> mappingTables, string version)
    {
        return mappingTables
            .Where(c => c.Version == version)
            .SingleOrDefault();
    }

    private ObjectResult MappingTableNotFound(string identifier, string? version = null)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "MappingTable not found.",
            detail: string.IsNullOrWhiteSpace(version)
                ? $"No mappingTable with identifier '{identifier}' exists."
                : $"No mappingTable with identifier '{identifier}' and version '{version}' exists.");
    }

    private ObjectResult InvalidVersionFormat()
    {
        return Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Invalid version format.",
            detail: "The 'version' parameter must be a valid semantic version (e.g. 1.2.3).");
    }
}