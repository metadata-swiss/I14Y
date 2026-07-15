using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Serialization.Json;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Partner.Business.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text.Json;

namespace Bfs.Iop.Partner.Api.Controllers;

/// <summary>
/// Controller for export data for DCAT catalogs
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CatalogsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public CatalogsController(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient;

    /// <summary>
    /// Exports the catalog content in one of the standard formats.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="dataFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{catalogId}/dcat/exports/{dataFormat}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [Produces("application/rdf+xml", "application/x-turtle")]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok]
    public async Task<IActionResult> ExportDcatCatalog(
        Guid catalogId,
        CatalogExportFormat dataFormat,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDcatCatalogsExportByIdAndDataFormatAsync(catalogId, dataFormat, cancellationToken);

        var content = response.Result;
        var contentType = response.Headers[HeaderNames.ContentType].Single();

        return Content(content, contentType);
    }

    /// <summary>
    /// Gets the catalog with the given id.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{catalogId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DataWrapper<DcatCatalogModel>))]
    public async Task<DataWrapper<DcatCatalogModel>> GetDcatCatalog(Guid catalogId, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsByIdAsync(catalogId, cancellationToken);

        return response.Result.Wrap();
    }

    /// <summary>
    /// Gets a list of catalogs.
    /// </summary>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(DataWrapper<ICollection<DcatCatalogModel>>))]
    public async Task<DataWrapper<ICollection<DcatCatalogModel>>> GetDcatCatalogs(
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDcatCatalogsByPageAndPageSizeAsync(
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
    /// Gets all the records from a specific catalog.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{catalogId:guid}/records")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DataWrapper<ICollection<DcatCatalogRecordModel>>))]
    public async Task<DataWrapper<ICollection<DcatCatalogRecordModel>>> GetDcatCatalogRecords(
        Guid catalogId,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsRecordsByIdAndPageAndPageSizeAsync(catalogId, page, pageSize, cancellationToken);

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
    /// Gets a specific catalog record.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="recordId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{catalogId:guid}/records/{recordId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DataWrapper<DcatCatalogRecordModel>))]
    public async Task<DataWrapper<DcatCatalogRecordModel>> GetDcatCatalogRecord(Guid catalogId, Guid recordId, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsRecordsByIdAndRecordIdAsync(catalogId, recordId, cancellationToken);

        return response.Result.Wrap();
    }

    /// <summary>
    /// Creates a new catalog record.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="inputModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{catalogId:guid}/records")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Created]
    public async Task<ActionResult> PostDcatCatalogRecord(Guid catalogId, object inputModel, CancellationToken cancellationToken)
    {
        // This abomination is temporary and it is necessary to accept both a single object and an array of objects.
        // This should be deleted soon, once the clients are informed about the breaking change.
        // Don't forget to delete the class DcatCatalogRecordInputModelExamplesProvider as well.

        IEnumerable<DcatCatalogRecordInputModel> data = [];

        var text = inputModel.ToString() ?? string.Empty;

        try
        {
            var model = IopJsonSerializer.Deserialize<DcatCatalogRecordInputModel>(text);

            if (model is not null)
            {
                data = [model];
            }        
        }
        catch
        {
            data = IopJsonSerializer.Deserialize<IEnumerable<DcatCatalogRecordInputModel>>(text);
        }

        _ = await _apiClient.PostDcatCatalogsRecordsByIdAndBodyAsync(catalogId, data, cancellationToken);

        return Created();
    }

    /// <summary>
    /// Updates an existing catalog record.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="recordId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{catalogId:guid}/records/{recordId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutDcatCatalogRecord(Guid catalogId, Guid recordId, DataWrapper<DcatCatalogRecordInputModel> updateModel, CancellationToken cancellationToken)
    {
        _ = await _apiClient.PutDcatCatalogsRecordsByIdAndRecordIdAndBodyAsync(catalogId, recordId, updateModel.Data, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing catalog record with the specified ids.
    /// </summary>
    /// <param name="catalogId"></param>
    /// <param name="recordId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{catalogId:guid}/records/{recordId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteDcatCatalogRecord(
        Guid catalogId,
        Guid recordId,
        CancellationToken cancellationToken)
    {
        _ = await _apiClient.DeleteDcatCatalogsRecordsByIdAndRecordIdAsync(catalogId, recordId, cancellationToken);

        return NoContent();
    }
}
