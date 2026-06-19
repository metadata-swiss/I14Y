using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MappingTablesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MappingTablesController(IMediator mediator) => 
        _mediator = mediator;

    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(MappingTableModel))]
    public Task<MappingTableModel> GetMappingTable(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetMappingTableCommand(id), cancellationToken);

    /// <summary>
    /// Gets the mapping tables matching the given filters.
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
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(IEnumerable<MappingTableModel>))]
    public async Task<IEnumerable<MappingTableModel>> GetMappingTables(
        string? mappingTableIdentifier,
        string? publisherIdentifier,
        string? version,
        string? codeSystemUri,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetMappingTablesCommand(
                mappingTableIdentifier,
                publisherIdentifier,
                version,
                codeSystemUri,
                publicationLevel,
                registrationStatus,
                page,
                pageSize),
            cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Returns the information if the mapping table identifier is already in use and/or if the version already exists.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="version"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("exists/{identifier}/{version}")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [InternalServerError]
    [Ok(typeof(IdentifierVersionExistsModel))]
    public Task<IdentifierVersionExistsModel> GetIdentifierVersionExists(
        string identifier,
        string version,
        CancellationToken cancellationToken = default)
    {
        var command = new GetIdentifierVersionExistsCommand(identifier, version);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Gets the mapping relation with the specified ids.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="relationId">Mapping relation id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/relations/{relationId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(MappingRelationModel))]
    public Task<MappingRelationModel> GetMappingRelation(
        Guid id,
        Guid relationId,
        CancellationToken cancellationToken) => _mediator.Send(
            new GetMappingRelationCommand(id, relationId), cancellationToken);

    /// <summary>
    /// Gets all the relations from a specified mapping table.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        var pagedResult = await _mediator.Send(
            new GetMappingRelationsCommand(id, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }

    /// <summary>
    /// Gets the publication level information of a specific mapping table.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/publication-level")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(PublicationLevelInfoModel))]
    public Task<PublicationLevelInfoModel> GetPublicationLevelInfo(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetPublicationLevelInfoCommand(PublishableType.MappingTable, id), cancellationToken);

    /// <summary>
    /// Gets the registration status information of a specific mapping table.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/registration-status")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(RegistrationStatusInfoModel))]
    public Task<RegistrationStatusInfoModel> GetRegistrationStatusInfo(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetRegistrationStatusInfoCommand(PublishableType.MappingTable, id), cancellationToken);

    /// <summary>
    /// Creates a new mapping table.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [BadRequest]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostMappingTable(MappingTableInputModel input, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateMappingTableCommand(input), cancellationToken);
        return CreatedAtAction(nameof(GetMappingTable), new { id = result }, result);
    }

    /// <summary>
    /// Creates a new mapping table version.
    /// </summary>
    /// <param name="id">Previous mapping table id.</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{id:guid}/versions")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostMappingTableVersion(Guid id, MappingTableInputModel input, CancellationToken cancellationToken)
    {
        var command = new CreateMappingTableVersionCommand(id, input);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMappingTable), new { id = result }, result);
    }

    /// <summary>
    /// Creates new mapping relations and adds them to a specified mapping table.
    /// </summary>
    /// <param name="id">Mapping table id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{id:guid}/relations")]
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
        var command = new CreateMappingRelationsCommand(id, input);
        var result = await _mediator.Send(command, cancellationToken);
        return Created(nameof(GetMappingRelation), result);
    }

    /// <summary>
    /// Updates the publication level of the mapping table with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="level"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/publication-level")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid id,
        [FromQuery][Required] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelCommand(PublishableType.MappingTable, id, level),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level proposal of the mapping table with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/publication-level-proposal")]
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
        await _mediator.Send(
            new UpdatePublicationLevelProposalCommand(PublishableType.MappingTable, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of the mapping table with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid id,
        [FromQuery][Required] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusCommand(PublishableType.MappingTable, id, status),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status proposal of the mapping table with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/registration-status-proposal")]
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
        await _mediator.Send(
            new UpdateRegistrationStatusProposalCommand(PublishableType.MappingTable, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [BadRequest]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutMappingTable(
        Guid id,
        MappingTableInputModel updateModel,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMappingTableCommand(id, updateModel), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates an existing mapping relation.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="relationId">Codelist entry id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        var command = new UpdateMappingRelationCommand(id, relationId, input);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteMappingTable(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMappingTableCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes the mapping relation with the specified ids.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="relationId">Mapping relation id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        await _mediator.Send(new DeleteMappingRelationCommand(id, relationId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes all mapping relations from the mapping table with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        await _mediator.Send(new DeleteAllMappingRelationsCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Imports relations to a mapping table from a file upload.
    /// </summary>
    /// <param name="id">The id of the concept</param>
    /// <param name="dataFormat"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    [HttpPost("{id:guid}/relations/import")]
    [NoContent]
    [BadRequest]
    [Forbidden]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104857600)] // 100 MB
    public async Task<IActionResult> ImportMappingRelations(
        [FromRoute] Guid id,
        [FromQuery] [Required] MappingRelationsDataFormat dataFormat,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var stream = file.OpenReadStream();

        await _mediator.Send(new ImportMappingRelationsCommand(id, stream, dataFormat), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Exports relations from a mapping table in the chosen format.
    /// </summary>
    /// <param name="id">The id of the mapping table</param>
    /// <param name="dataFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}/relations/export")]
    [Produces("text/csv", "application/json", Type = typeof(FileStreamResult))]
    [Forbidden]
    [Unauthorized]
    [InternalServerError]
    public async Task<FileStreamResult> ExportMappingRelations (
        [FromRoute] Guid id,
        [FromQuery] [Required] MappingRelationsDataFormat dataFormat,
        CancellationToken cancellationToken)
    {
        var exportCommand = new ExportMappingRelationsCommand(id, dataFormat);
        var result = await _mediator.Send(exportCommand, cancellationToken);

        return File(result.Data, result.MimeType, result.FileName);
    }
}
