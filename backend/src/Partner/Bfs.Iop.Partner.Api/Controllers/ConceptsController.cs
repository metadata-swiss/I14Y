using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Serialization.Json;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Partner.Business.Extensions;
using Bfs.Iop.Partner.Business.Mappings;
using Bfs.Iop.Partner.Models.ConceptsInput;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bfs.Iop.Partner.Api.Controllers;

/// <summary>
/// Controller for managing concept data
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ConceptsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public ConceptsController(IIopCoreApiClient apiClient) =>
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Gets the concept with the specific id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="includeCodeListEntries">Valid only for concepts of the type CodeList</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{conceptId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(DataWrapper<IopConceptModel>))]
    public async Task<DataWrapper<IopConceptModel>> GetConcept(
        Guid conceptId,
        [FromQuery] bool includeCodeListEntries,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(
            conceptId, 
            includeCodeListEntries,
            cancellationToken);

        return response.Result.Wrap();
    }

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
    [Ok(typeof(DataWrapper<IEnumerable<IopConceptModel>>))]
    public async Task<DataWrapper<ICollection<IopConceptModel>>> GetConcepts(
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

        return response.Result.Wrap();
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
    public async Task<ActionResult<Guid>> PostConcept(DataWrapper<ConceptInputBase> input, CancellationToken cancellationToken)
    {
        var inputModel = input.Data.MapToIopConceptInputModel();

        var response = await _apiClient.PostConceptsByBodyAsync(inputModel, cancellationToken);
        var result = response.Result;

        return CreatedAtAction(nameof(GetConcept), new { conceptId = result }, result);
    }

    /// <summary>
    /// Updates an existing concept with the specified id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [Route("{conceptId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutConcept(
        Guid conceptId,
        DataWrapper<IopConceptInputModel> updateModel,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutConceptsByIdAndBodyAsync(conceptId, updateModel.Data, cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Route("{conceptId}/locked")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutIsLocked(Guid conceptId, [Required][FromQuery] bool locked, CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutConceptsLockedByIdAndLockedAsync(conceptId, locked, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level proposal of the concept with the specified id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{conceptId}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid conceptId,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutConceptsPublicationLevelProposalByIdAndProposalAsync(conceptId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level of the concept with the specified id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="level"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{conceptId}/publication-level")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid conceptId,
        [Required][FromQuery] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutConceptsPublicationLevelByIdAndLevelAsync(conceptId, level, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status proposal of the concept with the specified id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{conceptId}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid conceptId,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutConceptsRegistrationStatusProposalByIdAndProposalAsync(conceptId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status of the concept with the specified id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="status"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{conceptId}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid conceptId,
        [Required][FromQuery] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutConceptsRegistrationStatusByIdAndStatusAsync(conceptId, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing concept with the specified id.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize]
    [Route("{conceptId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteConcept(Guid conceptId, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteConceptsByIdAsync(conceptId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves concept in JSON format by concept id.
    /// </summary>
    /// <param name="conceptId">The concept id (Guid)</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The concept</returns>
    [HttpGet("{conceptId}/exports/json")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileContentResult))]
    public async Task<IActionResult> ExportConceptByConceptId(Guid conceptId, CancellationToken cancellationToken)
    {
        var concept = (await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(
            conceptId,
            includeCodeListEntries: true,
            cancellationToken)).Result;
        
        var file = IopJsonSerializer.SerializeToFile($"Concept_{concept.Identifiers.FirstOrDefault(conceptId.ToString())}", concept);

        return File(file.Data, IopJsonSerializer.ContentType, file.FileName);
    }

    /// <summary>
    /// Retrieves the code list entries from a concept of the type CodeList by it's id.
    /// </summary>
    /// <param name="conceptId">The concept id (Guid)</param>
    /// <param name="dataFormat">File format to export.</param>
    /// <param name="withAnnotations">Bool flag for annotations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The code list entries</returns>
    [HttpGet("{conceptId}/codelist-entries/exports/{dataFormat}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<ActionResult> ExportConceptCodeListEntriesByConceptId(
        Guid conceptId, 
        [Required][FromRoute]CodeListEntriesDataFormat dataFormat = CodeListEntriesDataFormat.Json,
        bool withAnnotations = true,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesExportByIdAndDataFormatAndWithAnnotationsAsync(
            conceptId,
            dataFormat,
            withAnnotations,
            cancellationToken);

        var fileName = response.GetFileNameFromHeader();
        var contentType = response.GetContentTypeFromHeader();

        return File(response.Stream, contentType, fileName);
    }

    /// <summary>
    /// Imports concept code list entries by concept id.
    /// </summary>
    /// <param name="conceptId">The concept id (Guid)</param>
    /// <param name="file">The import file.</param>
    /// <param name="dataFormat">File format to import.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("{conceptId}/codelist-entries/imports/{dataFormat}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104857600)]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<ActionResult> ImportConceptCodeListEntriesByConceptId(
        Guid conceptId,
        IFormFile file,
        [Required] CodeListEntriesDataFormat dataFormat = CodeListEntriesDataFormat.Json,
        CancellationToken cancellationToken = default)
    {
        await _apiClient.PostConceptsCodelistEntriesImportByIdAndDataFormatAndBodyAsync(
            conceptId, 
            dataFormat,
            new FileParameter(
                file.OpenReadStream(), 
                file.FileName,
                file.ContentType),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returns the results of the search in code list entries of a specific concept. 
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="language"></param>
    /// <param name="query">
    /// The search query. The search is performed in the selected language only. 
    /// 
    /// You can use * to represent any number of characters and ? to represent a single character.
    /// Examples:
    /// - Agricultur*
    /// - Agricultur?
    /// </param>
    /// <param name="filters"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{conceptId}/codelist-entries/search")]
    [BadRequest]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DataWrapper<IEnumerable<CodeListEntryModel>>))]
    public async Task<DataWrapper<IEnumerable<CodeListEntryModel>>> SearchCodeListEntriesByConceptId(
        Guid conceptId,
        [Required][FromQuery] Language language,
        [FromQuery] string? query,
        [FromQuery]IEnumerable<string> filters,
        int? page, 
        int? pageSize, 
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesSearchByIdAndLanguageAndQueryAndFiltersAndAddCodeListEntriesPathsAndPageAndPageSizeAsync(
            conceptId,
            language.ToString().ToLowerInvariant(),
            query,
            filters,
            addCodeListEntriesPaths: false,
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

        return response.Result.Select(x => x.Entry).Wrap();
    }

    /// <summary>
    /// Exports the results of the search in code list entries of a specific concept.
    /// </summary>
    /// <param name="conceptId"></param>
    /// <param name="language"></param>
    /// <param name="query">
    /// The search query. The search is performed in the selected language only. 
    /// 
    /// You can use * to represent any number of characters and ? to represent a single character.
    /// Examples:
    /// - Agricultur*
    /// - Agricultur?
    /// </param>
    /// <param name="filters"></param>
    /// <param name="dataFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{conceptId}/codelist-entries/search/exports/{dataFormat}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<FileStreamResult> ExportSearchCodeListEntries(
        Guid conceptId,
        [Required][FromQuery] Language language,
        [FromQuery] string? query,
        [FromQuery] List<string> filters,
        [Required] CodeListEntriesDataFormat dataFormat,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesSearchExportByIdAndLanguageAndQueryAndFiltersAndDataFormatAsync(
            conceptId,
            language.ToString().ToLowerInvariant(),
            query,
            filters,
            dataFormat,
            cancellationToken);

        var fileName = response.GetFileNameFromHeader();
        var contentType = response.GetContentTypeFromHeader();

        return File(response.Stream, contentType, fileName);
    }

    /// <summary>
    /// Deletes all code list entries by concept id.
    /// </summary>
    /// <param name="conceptId">The concept id (Guid)</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("{conceptId}/codelist-entries")]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<ActionResult> DeleteAllConceptCodeListEntriesByConceptId(Guid conceptId, CancellationToken cancellationToken)
    {
        _ = await _apiClient.DeleteConceptsCodelistEntriesByIdAsync(conceptId, cancellationToken);

        return NoContent();
    }
}
