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
public class PublicServicesController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public PublicServicesController(IIopCoreApiClient apiClient) =>
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Gets the public service with the given id.
    /// </summary>
    /// <param name="publicServiceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{publicServiceId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(DataWrapper<PublicServiceModel>))]
    public async Task<DataWrapper<PublicServiceModel>> GetPublicService(Guid publicServiceId, CancellationToken cancellationToken) =>
        (await _apiClient.GetPublicServicesByIdAsync(publicServiceId, cancellationToken)).Result.Wrap();

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
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(DataWrapper<IEnumerable<PublicServiceModel>>))]
    public async Task<DataWrapper<ICollection<PublicServiceModel>>> GetDataServices(
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
    /// Creates a new public service.
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
    public async Task<ActionResult<Guid>> PostPublicService(DataWrapper<PublicServiceInputModel> input, CancellationToken cancellationToken)
    {
        var id = (await _apiClient.PostPublicServicesByBodyAsync(input.Data, cancellationToken)).Result;
        return CreatedAtAction(nameof(GetPublicService), new { publicServiceId = id }, id);
    }

    /// <summary>
    /// Updates an existing public service with the specified id.
    /// </summary>
    /// <param name="publicServiceId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [Route("{publicServiceId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutPublicService(
        Guid publicServiceId,
        DataWrapper<PublicServiceInputModel> updateModel,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesByIdAndBodyAsync(publicServiceId, updateModel.Data, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing public service with the specified id.
    /// </summary>
    /// <param name="publicServiceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize]
    [Route("{publicServiceId:guid}")]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeletePublicService(Guid publicServiceId, CancellationToken cancellationToken)
    {
        await _apiClient.DeletePublicServicesByIdAsync(publicServiceId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level of the public service with the specified id.
    /// </summary>
    /// <param name="publicServiceId">id of the public service</param>
    /// <param name="level">selection of possible publication levels</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{publicServiceId}/publication-level")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    public async Task<IActionResult> PutPublicationLevel(
        Guid publicServiceId,
        [Required][FromQuery] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesPublicationLevelByIdAndLevelAsync(publicServiceId, level, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the publication level proposal of the public service with the specified id
    /// </summary>
    /// <param name="publicServiceId">id of the public service</param>
    /// <param name="proposal">selection of possible publication level proposals</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{publicServiceId}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid publicServiceId,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesPublicationLevelProposalByIdAndProposalAsync(publicServiceId, proposal, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status of the public service with the specified id.
    /// </summary>
    /// <param name="publicServiceId">id of the public service</param>
    /// <param name="status">selection of possible registration statuses</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{publicServiceId}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid publicServiceId,
        [Required][FromQuery] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesRegistrationStatusByIdAndStatusAsync(publicServiceId, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets the registration status proposal of the public service with the specified id.
    /// </summary>
    /// <param name="publicServiceId">id of the public service</param>
    /// <param name="proposal">selection of possible registration status proposals</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{publicServiceId}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid publicServiceId,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesRegistrationStatusProposalByIdAndProposalAsync(publicServiceId, proposal, cancellationToken);
        return NoContent();
    }
}
