using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
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
public sealed class DatasetsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public DatasetsController(IIopCoreApiClient apiClient) =>
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Gets the DCAT dataset with the given id.
    /// </summary>
    /// <param name="datasetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{datasetId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(DataWrapper<DcatDatasetModel>))]
    public async Task<DataWrapper<DcatDatasetModel>> GetDcatDataset(Guid datasetId, CancellationToken cancellationToken) =>
        (await _apiClient.GetDatasetsByIdAsync(datasetId, cancellationToken)).Result.Wrap();

    /// <summary>
    /// Gets the DCAT datasets matching the given filters.
    /// </summary>
    /// <param name="accessRights">Code from RightsStatement_ACCESS_RIGHTS vocabulary.</param>
    /// <param name="datasetIdentifier"></param>
    /// <param name="publisherIdentifier"></param>
    /// <param name="publicationLevel"></param>
    /// <param name="registrationStatus"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(DataWrapper<IEnumerable<DcatDatasetModel>>))]
    public async Task<DataWrapper<ICollection<DcatDatasetModel>>> GetDcatDatasets(
        string? accessRights,
        string? datasetIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDatasetsByAccessRightsAndDatasetIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            accessRights,
            datasetIdentifier,
            publisherIdentifier,
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
    /// Exports the dataset structure in a file format.
    /// </summary>
    /// <param name="datasetId"></param>
    /// <param name="dataFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{datasetId:guid}/structures/exports/{dataFormat}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<ActionResult> ExportDatasetStructures(
        Guid datasetId,
        [FromRoute] LinkedDataFormat dataFormat,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetsModelExportByIdAndFormatAsync(datasetId, dataFormat, cancellationToken);

        var fileName = response.GetFileNameFromHeader();
        var contentType = response.GetContentTypeFromHeader();

        return File(response.Stream, contentType, fileName);
    }

    /// <summary>
    /// Creates a new DCAT dataset.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Created]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [Authorize]
    public async Task<ActionResult<Guid>> PostDcatDataset(DataWrapper<DcatDatasetInputModel> input, CancellationToken cancellationToken)
    {
        var id = (await _apiClient.PostDatasetsByBodyAsync(input.Data, cancellationToken)).Result;
        return CreatedAtAction(nameof(GetDcatDataset), new { datasetId = id }, id);
    }

    [HttpPost]
    [Route("{datasetId:guid}/structures/imports")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> ImportDatasetStructures(Guid datasetId, IFormFile file, CancellationToken cancellationToken)
    {
        _ = await _apiClient.PostDatasetsModelImportByIdAndBodyAsync(
            datasetId, 
            new FileParameter(
                file.OpenReadStream(), 
                file.FileName, 
                file.ContentType), 
            cancellationToken);

        return NoContent();
    }

    [HttpDelete]
    [Route("{datasetId:guid}/structures")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteDatasetStructures(Guid datasetId, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteDatasetsModelDeleteByIdAsync(datasetId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates an existing DCAT dataset with the specified id.
    /// </summary>
    /// <param name="datasetId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [Route("{datasetId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutDcatDataset(
        Guid datasetId,
        DataWrapper<DcatDatasetInputModel> updateModel,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsByIdAndBodyAsync(datasetId, updateModel.Data, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing DCAT dataset with the specified id.
    /// </summary>
    /// <param name="datasetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize]
    [Route("{datasetId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteDcatDataset(Guid datasetId, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteDatasetsByIdAsync(datasetId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level of the dataset with the specified id.
    /// </summary>
    /// <param name="datasetId">id of the dataset</param>
    /// <param name="level">selection of possible publication levels</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{datasetId}/publication-level")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    public async Task<IActionResult> PutPublicationLevel(
        Guid datasetId, 
        [Required][FromQuery] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsPublicationLevelByIdAndLevelAsync(datasetId, level, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level proposal of the dataset with the specified id
    /// </summary>
    /// <param name="datasetId">id of the dataset</param>
    /// <param name="proposal">selection of possible publication level proposals</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{datasetId}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid datasetId, 
        [FromQuery] PublicationLevel? proposal, 
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsPublicationLevelProposalByIdAndProposalAsync(datasetId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status of the dataset with the specified id.
    /// </summary>
    /// <param name="datasetId">id of the dataset</param>
    /// <param name="status">selection of possible registration statuses</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{datasetId}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid datasetId,
        [Required][FromQuery] RegistrationStatus status, 
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsRegistrationStatusByIdAndStatusAsync(datasetId, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status proposal of the dataset with the specified id.
    /// </summary>
    /// <param name="datasetId">id of the dataset</param>
    /// <param name="proposal">selection of possible registration status proposals</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{datasetId}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid datasetId,
        [FromQuery] RegistrationStatus? proposal, 
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsRegistrationStatusProposalByIdAndProposalAsync(datasetId, proposal, cancellationToken);
        return NoContent();
    }
}
