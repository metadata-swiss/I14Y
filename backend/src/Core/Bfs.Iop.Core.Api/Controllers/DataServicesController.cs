using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DataServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DataServicesController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets the data service with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(DataServiceModel))]
    public Task<DataServiceModel> GetDataService(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetDataServiceCommand(id), cancellationToken);

    /// <summary>
    /// Gets the data service with the given identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("by-identifier/{identifier}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(DataServiceModel))]
    public Task<DataServiceModel> GetDataServiceByIdentifier(string identifier, CancellationToken cancellationToken) =>
        _mediator.Send(new GetDataServiceByIdentifierCommand(identifier), cancellationToken);

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
    [NotFound]
    [Forbidden]
    [Unauthorized]
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
        var result = await _mediator.Send(
            new GetDataServicesCommand(
                accessRights,
                dataServiceIdentifier,
                publisherIdentifier,
                publicationLevel,
                registrationStatus,
                page,
                pageSize),
            cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets the next versions from a specific data service.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        var result = await _mediator.Send(new GetDataServiceNextVersionsCommand(id, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets the publication level information of a specific data service.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/publication-level")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(PublicationLevelInfoModel))]
    public Task<PublicationLevelInfoModel> GetPublicationLevelInfo(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetPublicationLevelInfoCommand(PublishableType.DataService, id), cancellationToken);

    /// <summary>
    /// Gets the registration status information of a specific data service.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/registration-status")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [AllowAnonymous]
    [Ok(typeof(RegistrationStatusInfoModel))]
    public Task<RegistrationStatusInfoModel> GetRegistrationStatusInfo(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetRegistrationStatusInfoCommand(PublishableType.DataService, id), cancellationToken);

    /// <summary>
    /// Creates a new data service.
    /// </summary>
    /// <param name="inputModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [BadRequest]
    [NotFound]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostDataService(DataServiceInputModel inputModel, CancellationToken cancellationToken)
    {
        var command = new CreateDataServiceCommand(inputModel);
        var dataServiceId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetDataService), new { id = dataServiceId }, dataServiceId);
    }

    /// <summary>
    /// Updates an existing data service with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inputModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutDataService(
        Guid id,
        DataServiceInputModel inputModel,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDataServiceCommand(id, inputModel);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level of the data service with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="level"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/publication-level")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid id,
        [FromQuery][Required] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelCommand(PublishableType.DataService, id, level),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level proposal of the data service with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/publication-level-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid id,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelProposalCommand(PublishableType.DataService, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of the data service with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/registration-status")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid id,
        [FromQuery][Required] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusCommand(PublishableType.DataService, id, status),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status proposal of the data service with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proposal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/registration-status-proposal")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid id,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusProposalCommand(PublishableType.DataService, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing data service with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [Conflict]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteDataService(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDataServiceCommand(id);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}