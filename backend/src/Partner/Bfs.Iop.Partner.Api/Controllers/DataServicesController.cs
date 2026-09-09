using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Partner.Business.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bfs.Iop.Partner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class DataServicesController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public DataServicesController(IIopCoreApiClient apiClient) =>
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Gets the data service with the given id.
    /// </summary>
    /// <param name="dataServiceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{dataServiceId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(DataWrapper<DataServiceModel>))]
    public async Task<DataWrapper<DataServiceModel>> GetDataService(Guid dataServiceId, CancellationToken cancellationToken) =>
        (await _apiClient.GetDataServicesByIdAsync(dataServiceId, cancellationToken)).Result.Wrap();

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
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(DataWrapper<IEnumerable<DataServiceModel>>))]
    public async Task<DataWrapper<ICollection<DataServiceModel>>> GetDataServices(
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
    /// Creates a new data service.
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
    public async Task<ActionResult<Guid>> PostDataService(DataWrapper<DataServiceInputModel> input, CancellationToken cancellationToken)
    {
        var id = (await _apiClient.PostDataServicesByBodyAsync(input.Data, cancellationToken)).Result;
        return CreatedAtAction(nameof(GetDataService), new { dataServiceId = id }, id);
    }

    /// <summary>
    /// Updates an existing data service with the specified id.
    /// </summary>
    /// <param name="dataServiceId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [Route("{dataServiceId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutDataService(
        Guid dataServiceId,
        DataWrapper<DataServiceInputModel> updateModel,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDataServicesByIdAndBodyAsync(dataServiceId, updateModel.Data, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing data service with the specified id.
    /// </summary>
    /// <param name="dataServiceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize]
    [Route("{dataServiceId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteDataService(Guid dataServiceId, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteDataServicesByIdAsync(dataServiceId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level of the data service with the specified id.
    /// </summary>
    /// <param name="dataServiceId">id of the data service</param>
    /// <param name="level">selection of possible publication levels</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{dataServiceId}/publication-level")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    public async Task<IActionResult> PutPublicationLevel(
        Guid dataServiceId,
        [Required][FromQuery] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDataServicesPublicationLevelByIdAndLevelAsync(dataServiceId, level, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level proposal of the data service with the specified id.
    /// </summary>
    /// <param name="dataServiceId">id of the data service</param>
    /// <param name="proposal">selection of possible publication level proposals</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{dataServiceId}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid dataServiceId,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDataServicesPublicationLevelProposalByIdAndProposalAsync(dataServiceId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status of the data service with the specified id.
    /// </summary>
    /// <param name="dataServiceId">id of the data service</param>
    /// <param name="status">selection of possible registration statuses</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{dataServiceId}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid dataServiceId,
        [Required][FromQuery] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDataServicesRegistrationStatusByIdAndStatusAsync(dataServiceId, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status proposal of the data service with the specified id.
    /// </summary>
    /// <param name="dataServiceId">id of the data service</param>
    /// <param name="proposal">selection of possible registration status proposals</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{dataServiceId}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid dataServiceId,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDataServicesRegistrationStatusProposalByIdAndProposalAsync(dataServiceId, proposal, cancellationToken);
        return NoContent();
    }
}
