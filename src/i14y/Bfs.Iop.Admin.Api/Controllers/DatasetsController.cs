using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DatasetsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public DatasetsController(IIopCoreApiClient apiClient) => _apiClient = apiClient;

    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DcatDatasetModel))]
    public async Task<DcatDatasetModel> GetDataset(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetsByIdAsync(id, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Gets the  datasets matching the given filters.
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
    [EnableCors("AllowBIT")]
    [HttpGet()]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(IEnumerable<DcatDatasetModel>))]
    public async Task<IEnumerable<DcatDatasetModel>> GetDatasets(
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
            pageSize, cancellationToken);

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
    [Ok(typeof(IEnumerable<DcatDatasetModel>))]
    public async Task<IEnumerable<DcatDatasetModel>> GetNextVersions(
    Guid id,
    int? page,
    int? pageSize,
    CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDatasetsNextVersionsByIdAndPageAndPageSizeAsync(id, page, pageSize, cancellationToken);

        int pageHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.AddPagingHeaders(pageHeader, pageSizeHeader, totalCount);

        return response.Result;
    }

    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{datasetId:guid}/distributions/{distributionId:guid}/access-services")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<DataServiceModel>))]
    public async Task<IEnumerable<DataServiceModel>> GetDatasetDistributionAccessServices(
        Guid datasetId,
        Guid distributionId, 
        CancellationToken cancellationToken)
    {
        var page = 1;
        var pageSize = int.MaxValue;

        var response = await _apiClient.GetDatasetsDistributionsAccessServicesByDatasetIdAndDistributionIdAndPageAndPageSizeAsync(
            datasetId,
            distributionId, 
            page, 
            pageSize,
            cancellationToken);

        return response.Result;
    }
}
