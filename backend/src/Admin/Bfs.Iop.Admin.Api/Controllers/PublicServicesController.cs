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
public sealed class PublicServicesController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public PublicServicesController(IIopCoreApiClient apiClient) => _apiClient = apiClient;

    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(PublicServiceModel))]
    public async Task<PublicServiceModel> GetPublicService(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetPublicServicesByIdAsync(id, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Gets the public services matching the given filters.
    /// </summary>
    /// <param name="publicServiceIdentifier"></param>
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
    [Ok(typeof(IEnumerable<PublicServiceModel>))]
    public async Task<IEnumerable<PublicServiceModel>> GetPublicServices(
        string? publicServiceIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetPublicServicesByPublicServiceIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSizeAsync(
            publicServiceIdentifier,
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
    /// Checks whether a channel with the given identifier exists.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("channels/identifier/{identifier}/exists")]
    [Authorize()]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(bool))]
    public async Task<bool> GetChannelIdentifierExists(string identifier, CancellationToken cancellationToken)
    {
        try
        {
            _ = await _apiClient.GetPublicServicesChannelsByIdentifierByIdentifierAsync(identifier, cancellationToken);
            return true;
        }
        catch (ApiException ex)
        when (ex.StatusCode is StatusCodes.Status401Unauthorized or
                StatusCodes.Status403Forbidden or
                StatusCodes.Status404NotFound)
        {

            return ex.StatusCode is not StatusCodes.Status404NotFound;
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
    public async Task<ActionResult> ExportPublicService(Guid id, [FromRoute] DataFormat format, CancellationToken cancellationToken)
    {
        if (format is not DataFormat.Json)
        {
            throw new NotSupportedException($"The format '{format}' is not supported.");
        }

        var response = await _apiClient.GetPublicServicesByIdAsync(id, cancellationToken);

        var fileName = $"PublicService_{response.Result.Identifiers.First()}";
        var contentType = "application/json";

        var file = IopJsonSerializer.SerializeToFile(fileName, response.Result);

        return File(file.Data, contentType, file.FileName);
    }
}
