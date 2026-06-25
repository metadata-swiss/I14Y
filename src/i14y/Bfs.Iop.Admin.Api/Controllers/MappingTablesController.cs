using Bfs.Iop.Admin.Api.Extensions;
using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MappingTablesController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMediator _mediator;

    public MappingTablesController(IIopCoreApiClient apiClient, IMediator mediator)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets the mapping table with the specific id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(MappingTableModel))]
    public async Task<MappingTableModel> GetMappingTable(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesByIdAsync(
            id,
            cancellationToken);

        return response.Result;
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
    [EnableCors("AllowBIT")]
    [HttpGet()]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
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

        int pageHeaderValue = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeValue = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.AddPagingHeaders(pageHeaderValue, pageSizeValue, totalCount);

        return response.Result;
    }

    /// <summary>
    /// Gets the mapping relation with the specified ids.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="relationId">Mapping relation id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/relations/{relationId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(MappingRelationModel))]
    public async Task<MappingRelationModel> GetMappingRelation(
        Guid id,
        Guid relationId,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesRelationsByIdAndRelationIdAsync(id, relationId, cancellationToken);

        return response.Result;
    }

    /// <summary>
    /// Gets all the relations from a specified mapping table.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/relations")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<MappingRelationModel>))]
    public async Task<IEnumerable<MappingRelationModel>> GetMappingRelations(
        Guid id,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesRelationsByIdAndPageAndPageSizeAsync(
            id,
            page,
            pageSize,
            cancellationToken);

        int pageHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.AddPagingHeaders(pageHeader, pageSizeHeader, totalCount);

        return response.Result;
    }

    /// <summary>
    /// Gets the publication level of the concept.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/publicationLevel")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(PublicationLevelInfoModel))]
    public async Task<PublicationLevelInfoModel> GetPublicationLevel(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesPublicationLevelByIdAsync(id, cancellationToken);

        return response.Result;
    }

    /// <summary>
    /// Gets the registration status of the concept.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/registrationStatus")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(RegistrationStatusInfoModel))]
    public async Task<RegistrationStatusInfoModel> GetRegistrationStatus(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesRegistrationStatusByIdAsync(id, cancellationToken);

        return response.Result;
    }

    /// <summary>
    /// Checks whether the identifier with version is in use.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("identifier/{identifier}/{version}/exists")]
    [Authorize()]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(IdentifierVersionExistsResult))]
    public async Task<IdentifierVersionExistsResult> GetIdentifierAndVersionExists(string identifier, string version, CancellationToken cancellationToken)
    {
        var command = new GetIdentifierAndVersionExistsCommand(IdentifierVersionExistsResultObjectType.MappingTable, identifier, version);
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    /// <summary>
    /// Creates a new mapping table.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPost]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostMappingTable(MappingTableInputModel input, CancellationToken cancellationToken)
    {
        var response = await _apiClient.PostMappingTablesByBodyAsync(input, cancellationToken);
        var result = response.Result;

        return CreatedAtAction(nameof(GetMappingTable), new { id = result }, result);
    }

    /// <summary>
    /// Creates new mapping relations and adds them to a specified mapping table.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPost]
    [Route("{id:guid}/relations")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Forbidden]
    [Conflict]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<IEnumerable<Guid>>> PostMappingRelations(
        Guid id,
        IEnumerable<MappingRelationInputModel> input,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.PostMappingTablesRelationsByIdAndBodyAsync(id, input, cancellationToken);

        return Created(nameof(GetMappingRelation), response.Result);
    }

    /// <summary>
    /// Updates an existing mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [Authorize]
    [Route("{id:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutMappingTable(
        Guid id,
        MappingTableInputModel updateModel,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutMappingTablesByIdAndBodyAsync(id, updateModel, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates an existing mapping relation.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="relationId">Mapping relation id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [Authorize]
    [HttpPut]
    [Route("{id:guid}/relations/{relationId:guid}")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Forbidden]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutMappingRelation(
        Guid id,
        Guid relationId,
        MappingRelationInputModel input,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutMappingTablesRelationsByIdAndRelationIdAndBodyAsync(id, relationId, input, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level proposal of the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [Route("{id}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid id,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesPublicationLevelProposalByIdAndProposalAsync(id, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level of the mappingTable with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="level"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [Route("{id}/publication-level")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid id,
        [Required][FromQuery] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesPublicationLevelByIdAndLevelAsync(id, level, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status proposal of the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [Route("{id}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid id,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesRegistrationStatusProposalByIdAndProposalAsync(id, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status of the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [Route("{id}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid id,
        [Required][FromQuery] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutMappingTablesRegistrationStatusByIdAndStatusAsync(id, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpDelete]
    [Authorize]
    [Route("{id:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteMappingTable(Guid id, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteMappingTablesByIdAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes the mapping relation with the specified ids.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="relationId">Mapping relation id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpDelete]
    [Route("{id:guid}/relations/{relationId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteMappingRelation(
        Guid id,
        Guid relationId,
        CancellationToken cancellationToken)
    {
        await _apiClient.DeleteMappingTablesRelationsByIdAndRelationIdAsync(id, relationId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes all mapping relations from the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpDelete]
    [Route("{id:guid}/relations")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteAllMappingRelations(Guid id, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteMappingTablesRelationsByIdAsync(id, cancellationToken);
        return NoContent();
    }

    [EnableCors("AllowBIT")]
    [HttpPost("{id:guid}/relations/imports/{format}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104857600)]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<ActionResult> ImportMappingRelations(
        Guid id,
        [FromRoute] [Required] MappingRelationsDataFormat format,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await _apiClient.PostMappingTablesRelationsImportByIdAndDataFormatAndBodyAsync(
            id,
            format,
            new FileParameter(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType),
            cancellationToken);

        return NoContent();
    }

    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/relations/exports/{format}")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileContentResult))]
    public async Task<ActionResult> ExportMappingRelations(
        Guid id,
        [FromRoute] [Required] MappingRelationsDataFormat format,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetMappingTablesRelationsExportByIdAndDataFormatAsync(
            id,
            format,
            cancellationToken);

        var fileName = response.GetFileNameFromHeader();
        var contentType = response.GetContentTypeFromHeader();

        return File(response.Stream, contentType, fileName);
    }

    /// <summary>
    /// Create a new version based on a existing mapping table
    /// </summary>
    /// <param name="id">The previous mapping table id.</param>
    /// <param name="model">The new version information.</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost("{id:guid}/versions")]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [Created()]
    public async Task<ActionResult> PostCreateVersion(Guid id, MappingTableInputModel model, CancellationToken cancellationToken)
    {
        var response = await _apiClient.PostMappingTablesVersionsByIdAndBodyAsync(id, model, cancellationToken);
        return CreatedAtAction(nameof(GetMappingTable), new { id = response.Result }, response.Result);
    }
}
