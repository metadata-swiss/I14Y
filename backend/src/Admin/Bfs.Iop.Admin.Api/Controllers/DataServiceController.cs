using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The DataService controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataServiceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="DataServiceController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public DataServiceController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Gets the DataService by id
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}")]
    [ProducesJson]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(DataService))]
    public Task<DataService> GetDataService(Guid id, CancellationToken cancellationToken)
    {
        var command = new Commands.DataServiceView.GetByIdCommand(id);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Gets the registration status of the data service.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/registrationStatus")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(RegistrationStatusInfoModel))]
    public async Task<RegistrationStatusInfoModel> GetRegistrationStatus(Guid id, CancellationToken cancellationToken) =>
        (await _apiClient.GetDataServicesRegistrationStatusByIdAsync(id, cancellationToken)).Result;

    /// <summary>
    /// Get the publication level of the data service
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/publicationLevel")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Ok(typeof(PublicationLevelInfoModel))]
    public async Task<PublicationLevelInfoModel> GetPublicationLevel(Guid id, CancellationToken cancellationToken) =>
        (await _apiClient.GetDataServicesPublicationLevelByIdAsync(id, cancellationToken)).Result;
}