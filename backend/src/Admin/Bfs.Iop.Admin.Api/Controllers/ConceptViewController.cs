using Bfs.Iop.Admin.Api.Extensions;
using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.Commands.ConceptView;
using Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Serialization.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The conceptView controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConceptViewController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="ConceptViewController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public ConceptViewController(
        IMediator mediator,
        IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Gets the concepts matching the given filters.
    /// </summary>
    /// <param name="conceptIdentifier">An optional identifier used to filter concepts by their concept identifier.</param>
    /// <param name="publisherIdentifier">An optional identifier used to filter concepts by their publisher.</param>
    /// <param name="version">An optional version string used to filter concepts by version.</param>
    /// <param name="publicationLevel">An optional publication level used to filter concepts.</param>
    /// <param name="registrationStatus">An optional registration status used to filter concepts.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of concepts matching the specified filters.</returns>
    [HttpGet()]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(IEnumerable<IopConceptModel>))]
    public async Task<IEnumerable<IopConceptModel>> GetConcepts(
        string? conceptIdentifier,
        string? publisherIdentifier,
        string? version,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsByConceptIdentifierAndPublisherIdentifierAndVersionAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            conceptIdentifier,
            publisherIdentifier,
            version,
            publicationLevel,
            registrationStatus,
            page,
            pageSize, cancellationToken);

        int pageHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.AddPagingHeaders(pageHeader, pageSizeHeader, totalCount);

        return response.Result;
    }

    /// <summary>
    /// Gets all the versions from a Concept by its id.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/allVersions")]
    [AllowAnonymous]
    [ProducesJson]
    [NotFound]
    [BadRequest]
    [Ok(typeof(IEnumerable<ConceptVersionView>))]
    public async Task<ActionResult<IEnumerable<ConceptVersionView>>> GetAllConceptVersions(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetAllVersionsByIdCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the conceptView by id
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesJson]
    [NotFound]
    [Ok(typeof(ConceptView))]
    public async Task<ActionResult<ConceptView>> GetConceptView(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetByIdCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
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
    public async Task<PublicationLevelInfoModel> GetPublicationLevel(Guid id, CancellationToken cancellationToken) =>
        await _mediator.Send(new GetPublicationLevelByIdCommand(id), cancellationToken);

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
    public async Task<RegistrationStatusInfoModel> GetRegistrationStatus(Guid id, CancellationToken cancellationToken) =>
        await _mediator.Send(new GetRegistrationStatusByIdCommand(id), cancellationToken);

    /// <summary>
    /// Returns a boolean with the information if a codelist entry already exists in a concept.
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="code">The code value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if a code list entry with the given code exists in the concept; otherwise <c>false</c>.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/exists")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [Ok(typeof(bool))]
    public Task<bool> GetExist([FromRoute] Guid id, [Required] string code, CancellationToken cancellationToken)
    {
        var command = new GetCodeListEntryExistsCommand(id, code);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Returns all entries of the concept (including annotations)
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged collection of code list entry details including annotations.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries")]
    [ProducesJson]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [Ok(typeof(IEnumerable<CodeListEntryDetail>))]
    public async Task<IEnumerable<CodeListEntryDetail>> GetCodelistEntries(
        Guid id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var command = new GetAllPagedCommand(id, page, pageSize);
        var response = await _mediator.Send(command, cancellationToken);
        HttpContext.AddPagingHeaders(response.Page, response.PageSize, response.TotalCount);

        return response.Results;
    }

    /// <summary>
    /// Returns code list entries for the specified concept that match the given code prefix,
    /// with optional sorting and paging for use in auto-complete scenarios.
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="codePrefix">An optional prefix used to filter code list entries by their code value.</param>
    /// <param name="sortProperty">The sort property. When null, the default sort property defined in the concept will be used.</param>
    /// <param name="sortOrder">Default is sort ascending.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of code list entry details matching the specified filters and paging options.</returns>
    [HttpGet]
    [Route("{id:guid}/codelist-entries/auto-complete")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(IEnumerable<CodelistEntryInput>))]
    public async Task<IEnumerable<CodelistEntryInput>> GetCodeListEntriesForAutoComplete(
        [FromRoute] Guid id,
        [FromQuery] string codePrefix,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCodeListEntriesForAutoCompleteCommand(
            id, codePrefix, sortProperty, sortOrder, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);

        return result.Results;
    }

    /// <summary>
    /// Returns codelist entries of the concept filtered by code value
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="code">The code value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/by-code/{code}")]
    [ProducesJson]
    [BadRequest]
    [AllowAnonymous]
    [Ok(typeof(CodeListEntryDetail))]
    public async Task<CodeListEntryDetail> GetCodeListEntriesByIdByCodeValue(
        Guid id,
        string code,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCodeListEntryByCodeCommand(id, code), cancellationToken);

        return result;
    }

    /// <summary>
    /// Returns all root codelist entries of the codelist concept.
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="sortProperty">The sort property. When null, the default sort property defined in the concept will be used.</param>
    /// <param name="sortOrder">Default is sort ascending.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/by-root")]
    [ProducesJson]
    [BadRequest]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<CodeListEntryDetail>))]
    public async Task<IEnumerable<CodeListEntryDetail>> GetCodeListEntriesByIdByRoot(
        Guid id,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetPagedRootCodeListEntriesCommand(
            id,
            sortProperty,
            sortOrder,
            page,
            pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(response.Page, response.PageSize, response.TotalCount);

        return response.Results;
    }

    /// <summary>
    /// Returns all child codelist entries with the given parent_code.
    /// </summary>
    /// <param name="id">The id of the codelist concept.</param>
    /// <param name="code">The code value.</param>
    /// <param name="sortProperty">The sort property. When null, the default sort property defined in the concept will be used.</param>
    /// <param name="sortOrder">Default is sort ascending.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/children-of/{code}")]
    [ProducesJson]
    [BadRequest]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<CodeListEntryDetail>))]
    public async Task<IEnumerable<CodeListEntryDetail>> GetCodeListEntriesChildrenOfByIdByRoot(
        Guid id,
        string code,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetPagedChildrenCodeListEntriesCommand(
            id,
            code,
            sortProperty,
            sortOrder,
            page,
            pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(response.Page, response.PageSize, response.TotalCount);

        return response.Results;
    }

    /// <summary>
    /// Returns if the codelist concept has any parent code values.
    /// </summary>
    /// <param name="id">The id of the codelist concept.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/has-parent-codes")]
    [ProducesJson]
    [BadRequest]
    [AllowAnonymous]
    [Ok(typeof(bool))]
    public async Task<bool> GetCodeListEntriesHasParentCodes(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesHasParentCodesByIdAsync(
            id,
            cancellationToken);

        return response.Result;
    }

    /// <summary>
    /// Returns the page number of the code with the same parent code.
    /// </summary>
    /// <param name="id">The id of the codelist concept.</param>
    /// <param name="code">The code value.</param>
    /// <param name="sortProperty">The sort property. When null, the default sort property defined in the concept will be used.</param>
    /// <param name="sortOrder">Default is sort ascending.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/page-number-from-same-parent/{code}")]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [AllowAnonymous]
    [Ok(typeof(int))]
    public async Task<int> GetCodeListEntriesPageNumberFromSameParentByIdByRoot(
        Guid id,
        string code,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var getPageNumberFromSameParentResponse = 
            await _apiClient.GetConceptsCodelistEntriesPageNumberFromSameParentByIdAndCodeAndSortPropertyAndSortOrderAndPageSizeAsync(
            id,
            code,
            sortProperty,
            sortOrder,
            pageSize,
            cancellationToken);

        return getPageNumberFromSameParentResponse.Result;
    }

    /// <summary>
    /// Exports all code list entries of the concept in the specified format.
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="format">The data format in which to export the code list entries.</param>
    /// <param name="withAnnotations">When <c>true</c>, annotations are included in the export.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A file containing the exported code list entries in the requested format.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/codelist-entries/exports/{format}")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileContentResult))]
    public async Task<ActionResult> ExportConceptCodeListEntries(
        Guid id, 
        [FromRoute] [Required] CodeListEntriesDataFormat format,
        bool withAnnotations = true,
        CancellationToken cancellationToken=default)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesExportByIdAndDataFormatAndWithAnnotationsAsync(
            id,
            format,
            withAnnotations,
            cancellationToken);

        var fileName = response.GetFileNameFromHeader();
        var contentType = response.GetContentTypeFromHeader();

        return File(response.Stream, contentType, fileName);
    }

    /// <summary>
    /// Searches code list entries for a given concept and language, applying optional query text,
    /// filters, and paging parameters.
    /// </summary>
    /// <param name="id">The id of the codelist concept.</param>
    /// <param name="language">The language code used to localize the code list entries.</param>
    /// <param name="query">Optional free-text query used to filter the code list entries.</param>
    /// <param name="filters">Optional list of filter expressions applied when searching code list entries.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of code list entry search results for the specified concept.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/codelist-entries/search")]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IEnumerable<CodeListEntrySearchResultEntryModel>))]
    public async Task<IEnumerable<CodeListEntrySearchResultEntryModel>> SearchCodeListEntries(
        Guid id,
        [Required][FromQuery] string language,
        [FromQuery] string? query,
        [FromQuery] List<string> filters,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesSearchByIdAndLanguageAndQueryAndFiltersAndAddCodeListEntriesPathsAndPageAndPageSizeAsync(
            id,
            language,
            query,
            filters,
            addCodeListEntriesPaths: true,
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
    /// Gets the dataset attributes whose dcterms:conformsTo value references the specified concept
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="page" example="1">The number of the result page to return.</param>
    /// <param name="pageSize" example="25">The size of each result page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of dataset and attribute URIs where the attribute conforms to the concept</returns>
    [HttpGet]
    [Route("{id:guid}/structure-references")]
    [ProducesJson]
    [BadRequest]
    [Forbidden]
    [InternalServerError]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<IopConceptStructureReferenceModel>))]
    public async Task<IEnumerable<IopConceptStructureReferenceModel>> GetConceptStructureReferences(
        Guid id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsStructureReferencesByIdAndPageAndPageSizeAsync(
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
    /// Gets the number of dataset attributes whose dcterms:conformsTo value references the specified concept
    /// </summary>
    /// <param name="id">The id of the concept.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of dataset and attribute URIs where the attribute conforms to the concept</returns>
    [HttpGet]
    [Route("{id:guid}/structure-references/count")]
    [BadRequest]
    [Forbidden]
    [InternalServerError]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(int))]
    public async Task<int> GetConceptStructureReferencesCount(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsStructureReferencesByIdAndPageAndPageSizeAsync(
            id,
            1,
            int.MaxValue,
            cancellationToken);

        return response.Result.Count;
    }

    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/export/{format}")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<ActionResult> ExportConcept(Guid id, [FromRoute] DataFormat format, CancellationToken cancellationToken)
    {
        if (format is not DataFormat.Json)
        {
            throw new NotSupportedException($"The format '{format}' is not supported.");
        }

        var concept = (await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(
           id,
           includeCodeListEntries: true,
           cancellationToken)).Result;

        var file = IopJsonSerializer.SerializeToFile($"Concept_{concept.Identifiers.First()}", concept);

        return File(file.Data, IopJsonSerializer.ContentType, file.FileName);
    }
}