using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Admin.Commands.ConceptInput;
using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;
using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;
using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The concept input controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConceptInputController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="ConceptInputController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public ConceptInputController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Gets the entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}")]
    [ProducesJson]
    [AllowAnonymous]
    [Ok]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    public async Task<ConceptInput> Get(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetInputCommand<ConceptInput>(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result;
    }

    /// <summary>
    /// Checks whether the identifier with version is in use
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
        var command = new GetIdentifierAndVersionExistsCommand(IdentifierVersionExistsResultObjectType.Concept, identifier, version);
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    /// <summary>
    /// Checks whether the identifier is in use
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("identifier/{identifier}/Counter")]
    [Authorize()]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(bool))]
    public async Task<ActionResult<bool>> GetIdentifierCounter(string identifier, CancellationToken cancellationToken)
    {
        var command = new ExistsMoreThanOneConceptForIdentifierCommand(identifier);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Checks whether the identifier is in use
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("identifier/{identifier}/exists")]
    [Authorize()]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(bool))]
    public async Task<ActionResult<bool>> GetIdentifierExists(string identifier, CancellationToken cancellationToken)
    {
        var command = new GetByConceptIdentifierCommand(identifier);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add a new concept
    /// </summary>
    /// <param name="model">The new concept</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost]
    [ProducesJson]
    [Unauthorized]
    [BadRequest]
    [Created()]
    public async Task<ActionResult> Post(ConceptInput model, CancellationToken cancellationToken)
    {
        var command = new PostInputCommand<ConceptInput>(model);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates the entity
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [NoContent]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    public async Task<ActionResult> Put(ConceptInput model, CancellationToken cancellationToken)
    {
        var command = new PutInputCommand<ConceptInput>(model);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Locks or unlocks the concept with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="locked">True to lock, false to unlock.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [Route("{id:guid}/locked")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutIsLocked(Guid id, [FromQuery][Required]bool locked, CancellationToken cancellationToken)
    {
        await _apiClient.PutConceptsLockedByIdAndLockedAsync(id, locked, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes the entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpDelete("{id:guid}")]
    [NoContent]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteConceptsByIdAsync(id, cancellationToken);

        return NoContent();
    }

    #region RegistrationStatus

    /// <summary>
    /// Updates the registration proposal of a concept.
    /// </summary>
    /// <param name="id">The concept id.</param>
    /// <param name="proposal">The new registration proposal or an empty value to reset an existing one.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/registrationStatusProposal")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdateRegistrationStatusProposal([FromRoute] Guid id, RegistrationStatus? proposal, CancellationToken cancellationToken)
    {
        await _apiClient.PutConceptsRegistrationStatusProposalByIdAndProposalAsync(id, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of a concept.
    /// </summary>
    /// <param name="id">The concept id.</param>
    /// <param name="status">The new registration status.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/registrationStatus")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdateRegistrationStatus([FromRoute] Guid id, RegistrationStatus status, CancellationToken cancellationToken)
    {
        await _apiClient.PutConceptsRegistrationStatusByIdAndStatusAsync(id, status, cancellationToken);
        return NoContent();
    }

    #endregion RegistrationStatus

    #region PublicationLevel

    /// <summary>
    /// Updates the publication level proposal of a concept.
    /// </summary>
    /// <param name="id">The concept id.</param>
    /// <param name="proposal">The new publication level proposal or an empty value to reset an existing one.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/publicationLevelProposal")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdatePublicationLevelProposal([FromRoute] Guid id, PublicationLevel? proposal, CancellationToken cancellationToken)
    {
        await _apiClient.PutConceptsPublicationLevelProposalByIdAndProposalAsync(id, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the publication level of a concept.
    /// </summary>
    /// <param name="id">The concept id.</param>
    /// <param name="level">The new publication level.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/publicationLevel")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdatePublicationLevel([FromRoute] Guid id, PublicationLevel level, CancellationToken cancellationToken)
    {
        await _apiClient.PutConceptsPublicationLevelByIdAndLevelAsync(id, level, cancellationToken);
        return NoContent();
    }

    #endregion PublicationLevel

    #region CodelistEntry

    /// <summary>
    /// Returns the input model of a specific codelist entry .
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/{codeListEntryId:guid}")]
    [ProducesJson]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [Ok(typeof(CodelistEntryInput))]
    public Task<CodelistEntryInput> GetCodelistEntryInput(
        [FromRoute]Guid id,
        [FromRoute]Guid codeListEntryId,
        CancellationToken cancellationToken)
    {
        var command = new GetInputCommand(id, codeListEntryId);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Deletes one codelist entry from a concept
    /// </summary>
    /// <param name="id">the id from the concept</param>
    /// <param name="codeListEntryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpDelete("{id:guid}/codelist-entries/{codeListEntryId:guid}")]
    [BadRequest]
    [NoContent]
    [Unauthorized]
    [Forbidden]
    public async Task<IActionResult> DeleteCodeListEntry(Guid id, Guid codeListEntryId, CancellationToken cancellationToken)
    {
        _ = await _apiClient.DeleteConceptsCodelistEntriesByIdAndCodeListEntryIdAsync(id, codeListEntryId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes all codelist entries from a concept
    /// </summary>
    /// <param name="id">the id from the concept</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpDelete("{id:guid}/codelist-entries")]
    [BadRequest]
    [NoContent]
    [Unauthorized]
    [Forbidden]
    public async Task<IActionResult> DeleteAllConceptCodeListEntries(Guid id, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteConceptsCodelistEntriesByIdAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Add a new codelistEntry
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpPost("{id:guid}/codelist-entries")]
    [ProducesJson]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [Created]
    public async Task<ActionResult<Guid>> PostCodelistEntry(Guid id, CodelistEntryInput model, CancellationToken cancellationToken)
    {
        var command = new AddCommand(id, model);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCodelistEntryInput), new { id, codeListEntryId = result }, result);
    }

    /// <summary>
    /// Add a new annotation to a codelistEntry
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpPost("{id:guid}/codelist-entries/{codeListEntryId:guid}/annotations")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [Created]
    public async Task<ActionResult<Guid>> PostAnnotation(
        Guid id, 
        Guid codeListEntryId, 
        AnnotationInputModel model,
        CancellationToken cancellationToken)
    {
        var command = new AddAnnotationCommand(id, codeListEntryId, model);
        await _mediator.Send(command, cancellationToken);
        return new CreatedResult();
    }

    /// <summary>
    /// Updates an existing codelistEntry
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/codelist-entries/{codeListEntryId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NoContent]
    public async Task<IActionResult> PutCodelistEntry(
        Guid id,
        Guid codeListEntryId,
        CodelistEntryInput model,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCommand(id, codeListEntryId, model);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates an existing annotation.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/codelist-entries/{codeListEntryId:guid}/annotations/{annotationId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NoContent]
    public async Task<IActionResult> PutAnnotation(
        Guid id,
        Guid codeListEntryId,
        Guid annotationId,
        Annotation model,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAnnotationCommand(id, codeListEntryId, annotationId, model);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates an existing annotation.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpDelete("{id:guid}/codelist-entries/{codeListEntryId:guid}/annotations/{annotationId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NoContent]
    public async Task<IActionResult> DeleteAnnotation(
        Guid id,
        Guid codeListEntryId,
        Guid annotationId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAnnotationCommand(id, codeListEntryId, annotationId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [EnableCors("AllowBIT")]
    [HttpPost("{id:guid}/codelist-entries/imports/{format}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104857600)]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<ActionResult> ImportConceptCodeListEntriesByConceptId(
        Guid id,
        [FromRoute] [Required] CodeListEntriesDataFormat format,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await _apiClient.PostConceptsCodelistEntriesImportByIdAndDataFormatAndBodyAsync(
            id,
            format,
            new FileParameter(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType),
            cancellationToken);

        return NoContent();
    }

    #endregion CodelistEntry

    #region CreateVersion

    /// <summary>
    /// Create a new version based on a existing Concept
    /// </summary>
    /// <param name="model">The new version information</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost("createVersion")]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Created()]
    public async Task<ActionResult> PostCreateVersion(ConceptInputCreateVersion model, CancellationToken cancellationToken)
    {
        var command = new CreateVersionCommand(model);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    #endregion CreateVersion
}