using Bfs.Iop.Admin.Models;
using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The PublicServiceView controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PublicServiceViewController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="PublicServiceViewController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public PublicServiceViewController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Gets the public service view by id.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}")]
    [ProducesJson]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(PublicServiceView))]
    public Task<PublicServiceView> GetPublicServiceView(Guid id, CancellationToken cancellationToken)
    {
        var command = new Commands.PublicServiceView.GetByIdCommand(id);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Gets the registration status of the public service.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/registrationStatus")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(RegistrationStatusInfoModel))]
    public async Task<RegistrationStatusInfoModel> GetRegistrationStatus(Guid id, CancellationToken cancellationToken) =>
        (await _apiClient.GetPublicServicesRegistrationStatusByIdAsync(id, cancellationToken)).Result;

    /// <summary>
    /// Gets the publication level of the public service.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/publicationLevel")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(PublicationLevelInfoModel))]
    public async Task<PublicationLevelInfoModel> GetPublicationLevel(Guid id, CancellationToken cancellationToken) =>
        (await _apiClient.GetPublicServicesPublicationLevelByIdAsync(id, cancellationToken)).Result;
}