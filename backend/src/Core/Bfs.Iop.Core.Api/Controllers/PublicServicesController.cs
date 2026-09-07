using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PublicServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicServicesController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets the public service with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "ApiCache")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(PublicServiceModel))]
    public Task<PublicServiceModel> GetPublicService(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetPublicServiceCommand(id), cancellationToken);


    /// <summary>
    /// Gets the public service with the given identifier.
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
    [Ok(typeof(PublicServiceModel))]
    public Task<PublicServiceModel> GetPublicServiceByIdentifier(string identifier, CancellationToken cancellationToken) =>
        _mediator.Send(new GetPublicServiceByIdentifierCommand(identifier), cancellationToken);

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
    [NotFound]
    [Forbidden]
    [Unauthorized]
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
        var result = await _mediator.Send(
            new GetPublicServicesCommand(
                publicServiceIdentifier,
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
    /// Gets the authorized datasets described at the public service.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/is-described-at")]
    [BadRequest]
    [NotFound]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IEnumerable<DcatDatasetModel>))]
    public async Task<IEnumerable<DcatDatasetModel>> GetPublicServiceIsDescribedAt(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetPublicServiceIsDescribedAtCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets the authorized public services related to the public service
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/relations")]
    [BadRequest]
    [NotFound]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IEnumerable<PublicServiceModel>))]
    public async Task<IEnumerable<PublicServiceModel>> GetPublicServiceRelations(
    Guid id,
    CancellationToken cancellationToken)
    {
        var command = new GetPublicServiceRelationsCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets the authorized public services required by the public service.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/requires")]
    [BadRequest]
    [NotFound]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IEnumerable<PublicServiceModel>))]
    public async Task<IEnumerable<PublicServiceModel>> GetPublicServiceRequires(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetPublicServiceRequiresCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Returns the channel containing the specified identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("channels/by-identifier/{identifier}")]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [InternalServerError]
    [Ok(typeof(ChannelModel))]
    public Task<ChannelModel> GetChannelByIdentifier(string identifier, CancellationToken cancellationToken) => 
        _mediator.Send(new GetChannelByIdentifierCommand(identifier), cancellationToken);

    /// <summary>
    /// Creates a new public service.
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
    public async Task<ActionResult<Guid>> PostPublicService(PublicServiceInputModel inputModel, CancellationToken cancellationToken)
    {
        var command = new CreatePublicServiceCommand(inputModel);
        var publicServiceId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetPublicService), new { id = publicServiceId }, publicServiceId);
    }

    /// <summary>
    /// Updates the public service with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
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
    public async Task<IActionResult> PutPublicService(
        Guid id,
        PublicServiceInputModel updateModel,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePublicServiceCommand(id, updateModel);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets the publication level information of a specific public service.
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
        _mediator.Send(new GetPublicationLevelInfoCommand(PublishableResourceType.PublicService, id), cancellationToken);

    /// <summary>
    /// Gets the registration status information of a specific public service.
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
        _mediator.Send(new GetRegistrationStatusInfoCommand(PublishableResourceType.PublicService, id), cancellationToken);

    /// <summary>
    /// Updates the publication level proposal of the public service with the given id.
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
            new UpdatePublicationLevelProposalCommand(PublishableResourceType.PublicService, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level of the public service with the given id.
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
            new UpdatePublicationLevelCommand(PublishableResourceType.PublicService, id, level),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status proposal of the public service with the given id.
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
            new UpdateRegistrationStatusProposalCommand(PublishableResourceType.PublicService, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of the public service with the given id.
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
            new UpdateRegistrationStatusCommand(PublishableResourceType.PublicService, id, status),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing public service with the specified id.
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
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeletePublicService(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeletePublicServiceCommand(id);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
