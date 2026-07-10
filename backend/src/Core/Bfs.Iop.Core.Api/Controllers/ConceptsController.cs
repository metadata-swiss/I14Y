using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ConceptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConceptsController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets the concept with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeCodeListEntries">Valid only for concepts of the type CodeList.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IopConceptModel))]
    public Task<IopConceptModel> GetConcept(
        Guid id,
        [FromQuery] bool includeCodeListEntries = false,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetIopConceptCommand(id, includeCodeListEntries), cancellationToken);

    /// <summary>
    /// Gets the concepts matching the given filters.
    /// </summary>
    /// <param name="conceptIdentifier"></param>
    /// <param name="publisherIdentifier"></param>
    /// <param name="version"></param>
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
    [Ok(typeof(IEnumerable<IopConceptModel>))]
    public async Task<IEnumerable<IopConceptModel>> GetConcepts(
        string? conceptIdentifier,
        string? publisherIdentifier,
        string? version,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetIopConceptsCommand(
                conceptIdentifier,
                publisherIdentifier,
                version,
                publicationLevel,
                registrationStatus,
                page,
                pageSize),
            cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Returns the information if the concept identifier is already in use and/or if the version already exists.
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
    /// Gets the codelist entry with the specified ids.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="codeListEntryId">Codelist entry id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/{codeListEntryId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(CodeListEntryModel))]
    public Task<CodeListEntryModel> GetCodeListEntry(
        Guid id,
        Guid codeListEntryId,
        CancellationToken cancellationToken) => _mediator.Send(
            new GetCodeListEntryCommand(id, codeListEntryId), cancellationToken);

    /// <summary>
    /// Gets the codelist entry from a specified concept matching the given code.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="code">Codelist code</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/by-code")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(CodeListEntryModel))]
    public Task<CodeListEntryModel> GetCodeListEntryByCode(
        Guid id,
        [Required][FromQuery] string code,
        CancellationToken cancellationToken) => _mediator.Send(
            new GetCodeListEntryByCodeCommand(id, code), cancellationToken);

    /// <summary>
    /// Gets the codelist entry parents from a specified concept matching the given code.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="code">Codelist code</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/parents-by-code")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<CodeListEntrySearchResultPathModel>))]
    public Task<IEnumerable<CodeListEntrySearchResultPathModel>> GetCodeListEntryParentsByCode(
        Guid id,
        [Required][FromQuery] string code,
        CancellationToken cancellationToken) => _mediator.Send(
            new GetCodeListEntryParentsByCodeCommand(id, code), cancellationToken);

    /// <summary>
    /// Gets all codelist entries including annotations from a specified concept.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="sortProperty">When null, the property defined in concept metadata will be used.</param>
    /// <param name="sortOrder"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<CodeListEntryModel>))]
    public async Task<IEnumerable<CodeListEntryModel>> GetCodeListEntries(
        Guid id,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _mediator.Send(
            new GetCodeListEntriesCommand(id, sortProperty, sortOrder, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }

    /// <summary>
    /// Gets the root codelist entries from a specified concept.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="sortProperty">When null, the property defined in concept metadata will be used.</param>
    /// <param name="sortOrder"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/by-root")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<CodeListEntryModel>))]
    public async Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesByRoot(
        Guid id,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page, 
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _mediator.Send(
            new GetCodeListEntriesByRootCommand(id, sortProperty, sortOrder, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }

    /// <summary>
    /// Gets all child codelist entries from a given parent code.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="parentCode"></param>
    /// <param name="sortProperty">When null, the property defined in concept metadata will be used.</param>
    /// <param name="sortOrder"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/children-of")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<CodeListEntryModel>))]
    public async Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesChildrenOfParentCode(
        Guid id,
        [Required][FromQuery] string parentCode,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _mediator.Send(
            new GetCodeListEntriesChildrenOfParentCodeCommand(id, parentCode, sortProperty, sortOrder, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }

    /// <summary>
    /// Checks if the codelist has parent codes
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="cancellationToken"></param>
    /// <returns>true/false</returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/has-parent-codes")]
    [BadRequest]
    [Forbidden]
    [Unauthorized]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(bool))]
    public async Task<bool> GetCodeListEntriesHasParentCodes(
        [Required][FromRoute] Guid id,
        CancellationToken cancellationToken)
        => await _mediator.Send(
            new GetCodeListEntriesHasParentCodesCommand(id), cancellationToken);

    /// <summary>
    /// Returns the page number of the code with the same parent code
    /// </summary>
    /// <param name="id"></param>
    /// <param name="code"></param>
    /// <param name="sortProperty">When null, the property defined in concept metadata will be used.</param>
    /// <param name="sortOrder"></param>
    /// <param name="pageSize" example="25">the size of each result page.</param>
    /// <param name="cancellationToken">the cancellationToken.</param>
    /// <returns></returns>
    [HttpGet("{id:guid}/codelist-entries/page-number-from-same-parent")]
    [AllowAnonymous]
    [Forbidden]
    [Unauthorized]
    [NotFound]
    [BadRequest]
    [Ok(typeof(int))]
    public async Task<int> GetCodeListsEntriesPageNumberFromSameParent(
        [Required][FromRoute] Guid id, 
        [Required][FromQuery] string code,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? pageSize, 
        CancellationToken cancellationToken = default)
        => await _mediator.Send(
            new GetCodeListEntriesPageNumberFromSameParentCommand(id, code, sortProperty, sortOrder, pageSize), cancellationToken);

    /// <summary>
    /// Gets all codelist entries which code starts with the given prefix.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="codePrefix"></param>
    /// <param name="sortProperty">When null, the property defined in concept metadata will be used.</param>
    /// <param name="sortOrder"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/auto-complete")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<CodeListEntryModel>))]
    public async Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesForAutoComplete(
        Guid id,
        [Required][FromQuery] string codePrefix,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _mediator.Send(
            new GetCodeListEntriesForAutoCompleteCommand(id, codePrefix, sortProperty, sortOrder, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <param name="query"></param>
    /// <param name="filters"></param>
    /// <param name="addCodeListEntriesPaths" example="false"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/search")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<CodeListEntrySearchResultEntryModel>))]
    [BadRequest]
    [InternalServerError]
    [Unauthorized]
    [Forbidden]
    public async Task<IEnumerable<CodeListEntrySearchResultEntryModel>> SearchCodeListEntries(
        Guid id,
        [Required][FromQuery] string language,
        [FromQuery] string? query,
        [FromQuery] List<string> filters,
        [FromQuery] bool addCodeListEntriesPaths,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _mediator.Send(new GetCodeListEntriesSearchCommand(
            id,
            language,
            query,
            filters,
            addCodeListEntriesPaths,
            page,
            pageSize),
            cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
        return pagedResult.Results;
    }

    /// <summary>
    /// Exports the result of a search in format of a file.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <param name="query"></param>
    /// <param name="filters"></param>
    /// <param name="dataFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/search/export")]
    [Produces("text/csv", "application/json", Type = typeof(FileStreamResult))]
    [BadRequest]
    [InternalServerError]
    [Unauthorized]
    [Forbidden]
    public async Task<FileStreamResult> ExportSearchCodeListEntries(
        Guid id,
        [Required][FromQuery] string language,
        [FromQuery] string? query,
        [FromQuery] List<string> filters,
        [Required][FromQuery] CodeListEntriesDataFormat dataFormat,
        CancellationToken cancellationToken = default)
    {
        var exportCommand = new ExportCodeListEntriesSearchCommand(id, language, query, filters, dataFormat);
        var result = await _mediator.Send(exportCommand, cancellationToken);

        return File(result.Data, result.MimeType, result.FileName);
    }

    /// <summary>
    /// Gets the publication level information of a specified concept.
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
        _mediator.Send(new GetPublicationLevelInfoCommand(PublishableType.IopConcept, id), cancellationToken);

    /// <summary>
    /// Gets the registration status information of a specified concept.
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
        _mediator.Send(new GetRegistrationStatusInfoCommand(PublishableType.IopConcept, id), cancellationToken);

    /// <summary>
    /// Gets the dataset attributes whose dcterms:conformsTo value references the specified concept
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The list of dataset and attribute URIs where the attribute conforms to the concept</returns>
    [HttpGet]
    [Route("{id:guid}/structure-references")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<IopConceptStructureReferenceModel>))]
    public async Task<IEnumerable<IopConceptStructureReferenceModel>> GetConceptStructureReferences(Guid id,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var command = new GetConceptStructureReferencesCommand(id, page, pageSize);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);

        return result.Results;
    }

    /// <summary>
    /// Creates a new CodeList, Date, Numeric or String concept.
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
    public async Task<ActionResult<Guid>> PostConcept(IopConceptInputModel input, CancellationToken cancellationToken)
    {
        var command = new CreateIopConceptCommand(input);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetConcept), new { id = result }, result);
    }

    /// <summary>
    /// Creates new codelist entries and adds them to a specified codelist concept.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{id:guid}/codelist-entries")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Forbidden]
    [Conflict]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<IEnumerable<Guid>>> PostCodeListEntries(
        Guid id,
        IEnumerable<CodeListEntryInputModel> input,
        CancellationToken cancellationToken)
    {
        var command = new CreateCodeListEntriesCommand(id, input);
        var result = await _mediator.Send(command, cancellationToken);
        return Created(nameof(GetCodeListEntry), result);
    }

    /// <summary>
    /// Updates an existing codelist entry.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="codeListEntryId">Codelist entry id</param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/codelist-entries/{codeListEntryId:guid}")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Forbidden]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutCodeListEntry(
        Guid id,
        Guid codeListEntryId,
        CodeListEntryInputModel input,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCodeListEntryCommand(id, codeListEntryId, input);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level proposal of the concept with the given id.
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
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid id,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelProposalCommand(PublishableType.IopConcept, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level of the concept with the given id.
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
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid id,
        [FromQuery][Required] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelCommand(PublishableType.IopConcept, id, level),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status proposal of the concept with the given id.
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
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid id,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusProposalCommand(PublishableType.IopConcept, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of the concept with the given id.
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
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid id,
        [FromQuery][Required] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusCommand(PublishableType.IopConcept, id, status),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Locks or unlocks the concept with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="locked">True to lock, false to unlock.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/locked")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutIsLocked(Guid id, [FromQuery][Required] bool locked, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateIopConceptIsLockedCommand(id, locked), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes the concept with the specified id.
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
    public async Task<IActionResult> DeleteConcept(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteIopConceptCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes the codelist entry with the specified ids.
    /// </summary>
    /// <param name="id">Concept id</param>
    /// <param name="codeListEntryId">Codelist entry id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{id:guid}/codelist-entries/{codeListEntryId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteCodeListEntry(
        Guid id,
        Guid codeListEntryId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCodeListEntryCommand(id, codeListEntryId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes all codelist entries from the concept with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{id:guid}/codelist-entries")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteAllConceptCodeListEntries(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAllCodeListEntriesCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the concept with the specified id.
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
    public async Task<IActionResult> PutConcept(Guid id, IopConceptInputModel updateModel, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateIopConceptCommand(id, updateModel), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets the all versions of the concept with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/versions")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<IopConceptModel>))]
    public Task<IEnumerable<IopConceptModel>> GetConceptVersions(
        Guid id,
        CancellationToken cancellationToken) =>
        _mediator.Send(new GetIopConceptVersionsCommand(id), cancellationToken);

    /// <summary>
    /// Creates a new concept version.
    /// </summary>
    /// <param name="id">Previous IOP concept id.</param>
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
    public async Task<ActionResult<Guid>> PostConceptVersion(Guid id, IopConceptInputModel input, CancellationToken cancellationToken)
    {
        var command = new CreateIopConceptVersionCommand(id, input);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetConcept), new { id = result }, result);
    }

    /// <summary>
    /// Gets all concepts by identifier
    /// </summary>
    [HttpGet]
    [Route("identifier/{identifier}")]
    [Unauthorized]
    [BadRequest]
    [NotFound]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<IopConceptModel>))]
    public async Task<ActionResult<bool>> GetConceptsByIdentifier(string identifier, CancellationToken cancellationToken)
    {
        var command = new GetIopConceptsByIdentifierCommand(identifier);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Imports codelist entries to a codelist concept from a file upload.
    /// </summary>
    /// <param name="id">The id of the concept</param>
    /// <param name="dataFormat"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    [HttpPost("{id:guid}/codelist-entries/import")]
    [Authorize]
    [NoContent]
    [BadRequest]
    [Forbidden]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104857600)] // 100 MB
    public async Task<IActionResult> ImportCodelistEntries(
        [FromRoute] Guid id,
        [FromQuery] [Required] CodeListEntriesDataFormat dataFormat,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var stream = file.OpenReadStream();

        await _mediator.Send(new ImportCodelistEntriesCommand(id, stream, dataFormat), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Exports codelist entries from a concept in CSV format
    /// </summary>
    /// <param name="id">The id of the concept</param>
    /// <param name="dataFormat"></param>
    /// <param name="withAnnotations"></param>
    /// <param name="cancellationToken"></param>

    /// <returns></returns>
    [HttpGet("{id:guid}/codelist-entries/export")]
    [Produces("text/csv", "application/json", Type = typeof(FileStreamResult))]
    [Forbidden]
    [Unauthorized]
    [InternalServerError]
    public async Task<FileStreamResult> ExportConceptCodelistEntries(
        [FromRoute] Guid id,
        [FromQuery] [Required] CodeListEntriesDataFormat dataFormat,
        [FromQuery(Name = "withAnnotations")] bool withAnnotations=true,
        CancellationToken cancellationToken=default)
    {
        var exportCommand = new ExportCodeListEntriesCommand(id, dataFormat, withAnnotations);
        var result = await _mediator.Send(exportCommand, cancellationToken);

        return File(result.Data, result.MimeType, result.FileName);
    }
}