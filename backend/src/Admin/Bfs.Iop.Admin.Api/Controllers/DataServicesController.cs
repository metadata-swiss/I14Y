using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Serialization.Json;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DataServicesController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public DataServicesController(IIopCoreApiClient apiClient) => _apiClient = apiClient;

    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DataServiceModel))]
    public async Task<DataServiceModel> GetDataService(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDataServicesByIdAsync(id, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Gets the data services matching the given filters.
    /// </summary>
    /// <param name="accessRights">Code from RightsStatement_ACCESS_RIGHTS vocabulary.</param>
    /// <param name="dataServiceIdentifier"></param>
    /// <param name="publisherIdentifier"></param>
    /// <param name="publicationLevel"></param>
    /// <param name="registrationStatus"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet()]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(IEnumerable<DataServiceModel>))]
    public async Task<IEnumerable<DataServiceModel>> GetDataServices(
        string? accessRights,
        string? dataServiceIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDataServicesByAccessRightsAndDataServiceIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            accessRights,
            dataServiceIdentifier,
            publisherIdentifier,
            publicationLevel,
            registrationStatus,
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
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/next-versions")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<DataServiceModel>))]
    public async Task<IEnumerable<DataServiceModel>> GetNextVersions(
        Guid id,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDataServicesNextVersionsByIdAndPageAndPageSizeAsync(id, page, pageSize, cancellationToken);

        int pageHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.AddPagingHeaders(pageHeader, pageSizeHeader, totalCount);

        return response.Result;
    }

    [EnableCors("AllowBIT")]
    [HttpGet("identifier/{identifier}/exists")]
    [Authorize]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(bool))]
    public async Task<bool> GetIdentifierExists(string identifier, CancellationToken cancellationToken)
    {
        try
        {
            _ = await _apiClient.GetDataServicesByIdentifierByIdentifierAsync(identifier, cancellationToken);
            return true;
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode is
                StatusCodes.Status401Unauthorized or
                StatusCodes.Status403Forbidden or
                StatusCodes.Status404NotFound)
            {
                return ex.StatusCode is not StatusCodes.Status404NotFound;
            }

            throw;
        }
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
    public async Task<ActionResult> ExportDataService(Guid id, [FromRoute] DataFormat format, CancellationToken cancellationToken)
    {
        if (format is not DataFormat.Json)
        {
            throw new NotSupportedException($"The format '{format}' is not supported.");
        }

        var dataService = (await _apiClient.GetDataServicesByIdAsync(
            id,
            cancellationToken)).Result;

        var file = IopJsonSerializer.SerializeToFile($"DataService_{dataService.Identifiers.First()}", dataService);

        return File(file.Data, "application/json", file.FileName);
    }
}
