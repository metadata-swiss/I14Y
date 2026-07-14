using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Partner.Business.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bfs.Iop.Partner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class MappingTablesController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public MappingTablesController(IIopCoreApiClient apiClient) =>
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Gets the mapping table with the specific id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{mappingTableId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(DataWrapper<MappingTableModel>))]
    public async Task<DataWrapper<MappingTableModel>> GetMappingTable(
        Guid mappingTableId,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesByIdAsync(
            mappingTableId,
            cancellationToken);

        return response.Result.Wrap();
    }

    /// <summary>
    /// Gets mapping tables matching the given filters.
    /// </summary>
    /// <param name="mappingTableIdentifier"></param>
    /// <param name="publisherIdentifier"></param>
    /// <param name="version"></param>
    /// <param name="codeSystemUri"></param>
    /// <param name="publicationLevel"></param>
    /// <param name="registrationStatus"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Collection of mapping tables.</returns>
    [HttpGet()]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(IEnumerable<MappingTableModel>))]
    public async Task<IEnumerable<MappingTableModel>> GetMappingTables(
        [FromQuery] string? mappingTableIdentifier,
        [FromQuery] string? publisherIdentifier,
        [FromQuery] string? version,
        [FromQuery] string? codeSystemUri,
        [FromQuery] PublicationLevel? publicationLevel,
        [FromQuery] RegistrationStatus? registrationStatus,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesByMappingTableIdentifierAndPublisherIdentifierAndVersionAndCodeSystemUriAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            mappingTableIdentifier,
            publisherIdentifier,
            version,
            codeSystemUri,
            publicationLevel,
            registrationStatus,
            page,
            pageSize,
            cancellationToken);

        var pageHeaderValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageHeaderKey);
        var pageSizeValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageSizeHeaderKey);
        var totalPagesValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalPagesHeaderKey);
        var totalRowsValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.Response.Headers.Append(HttpContextExtensions.PageHeaderKey, pageHeaderValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.PageSizeHeaderKey, pageSizeValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalPagesHeaderKey, totalPagesValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalRowsHeaderKey, totalRowsValue);

        return response.Result;
    }

    /// <summary>
    /// Creates a new mapping table.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostMappingTable(DataWrapper<MappingTableInputModel> input, CancellationToken cancellationToken)
    {
        var response = await _apiClient.PostMappingTablesByBodyAsync(input.Data, cancellationToken);
        var result = response.Result;

        return CreatedAtAction(nameof(GetMappingTable), new { mappingTableId = result }, result);
    }

    /// <summary>
    /// Updates an existing mapping table with the specified id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [Route("{mappingTableId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutMappingTable(
        Guid mappingTableId,
        DataWrapper<MappingTableInputModel> updateModel,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutMappingTablesByIdAndBodyAsync(mappingTableId, updateModel.Data, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level proposal of the mapping table with the specified id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{mappingTableId}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid mappingTableId,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesPublicationLevelProposalByIdAndProposalAsync(mappingTableId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level of the mappingTable with the specified id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="level"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{mappingTableId}/publication-level")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid mappingTableId,
        [Required][FromQuery] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesPublicationLevelByIdAndLevelAsync(mappingTableId, level, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status proposal of the mapping table with the specified id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{mappingTableId}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid mappingTableId,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesRegistrationStatusProposalByIdAndProposalAsync(mappingTableId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status of the mapping table with the specified id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="status"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{mappingTableId}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid mappingTableId,
        [Required][FromQuery] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesRegistrationStatusByIdAndStatusAsync(mappingTableId, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing mapping table with the specified id.
    /// </summary>
    /// <param name="mappingTableId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize]
    [Route("{mappingTableId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteMappingTable(Guid mappingTableId, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteMappingTablesByIdAsync(mappingTableId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes all the relations from a mapping table with the specified id.
    /// </summary>
    /// <param name="mappingTableId">The mapping table id (Guid)</param> 
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{mappingTableId:guid}/relations")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteMappingRelations(
        Guid mappingTableId,
        CancellationToken cancellationToken)
    {
        await _apiClient.DeleteMappingTablesRelationsByIdAsync(
            mappingTableId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Retrieves the relations from a mapping table it's id.
    /// </summary>
    /// <param name="mappingTableId">The mapping table id (Guid)</param>
    /// <param name="dataFormat">File format to export.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The code list entries</returns>
    [HttpGet("{mappingTableId}/relations/exports/{dataFormat}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<ActionResult> ExportMappingRelationsByMappingTableId(
        Guid mappingTableId,
        [Required][FromRoute] MappingRelationsDataFormat dataFormat = MappingRelationsDataFormat.Json,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetMappingTablesRelationsExportByIdAndDataFormatAsync(
            mappingTableId,
            dataFormat,
            cancellationToken);

        var fileName = response.GetFileNameFromHeader();
        var contentType = response.GetContentTypeFromHeader();

        return File(response.Stream, contentType, fileName);
    }

    /// <summary>
    /// Imports relations by mapping table id.
    /// </summary>
    /// <param name="mappingTableId">The concept id (Guid)</param>
    /// <param name="file">The import file.</param>
    /// <param name="dataFormat">File format to import.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("{mappingTableId}/relations/imports/{dataFormat}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104857600)]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<ActionResult> ImportMappingRelationsByMappingTableId(
        Guid mappingTableId,
        IFormFile file,
        [Required] MappingRelationsDataFormat dataFormat = MappingRelationsDataFormat.Json,
        CancellationToken cancellationToken = default)
    {
        await _apiClient.PostMappingTablesRelationsImportByIdAndDataFormatAndBodyAsync(
            mappingTableId,
            dataFormat,
            new FileParameter(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Creates new mapping relations and adds them to a specified mapping table.
    /// </summary>
    /// <param name="mappingTableId">Mapping table id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{mappingTableId:guid}/relations")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Forbidden]
    [Conflict]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<IEnumerable<Guid>>> PostMappingTableRelations(
        Guid mappingTableId,
        DataWrapper<IEnumerable<MappingRelationInputModel>> input,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PostMappingTablesRelationsByIdAndBodyAsync(mappingTableId, input.Data, cancellationToken);

        return Created();
    }
}
