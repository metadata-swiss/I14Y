using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The public service input controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PublicServiceInputController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a <see cref="PublicServiceInputController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    /// <param name="mapper"></param>
    public PublicServiceInputController(IMediator mediator, IIopCoreApiClient apiClient, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Deletes the entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpDelete("{id:guid}")]
    [NoContent]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _ = await _apiClient.DeletePublicServicesByIdAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the entity
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPut]
    [NoContent]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    public async Task<ActionResult> Put(PublicServiceInput model, CancellationToken cancellationToken)
    {
        var command = new PutInputCommand<PublicServiceInput>(model);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }


    /// <summary>
    /// Checks whether the identifier is in use
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("identifier/{identifier}/exists")]
    [Authorize()]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(bool))]
    public Task<bool> GetIdentifierExists(string identifier, CancellationToken cancellationToken)
    {
        var command = new GetByPublicServiceIdentifierCommand(identifier);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Add a new public service
    /// </summary>
    /// <param name="model">The new public service</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost]
    [ProducesJson]
    [Unauthorized]
    [BadRequest]
    [Created()]
    public async Task<ActionResult> Post(PublicServiceInput model, CancellationToken cancellationToken)
    {
        var command = new PostInputCommand<PublicServiceInput>(model);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(PublicServicesController.GetPublicService), "PublicServices", new { id = result.Id }, result);
    }

    #region RegistrationStatus

    /// <summary>
    /// Updates the registration proposal of a public service.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="proposal">The new registration proposal or empty to revert an existing proposal.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/registrationStatusProposal")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdateRegistrationStatusProposal([FromRoute] Guid id, RegistrationStatus? proposal, CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesRegistrationStatusProposalByIdAndProposalAsync(
            id,
            proposal,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of a public service.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="status">The new registration status.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/registrationStatus")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdateRegistrationStatus([FromRoute] Guid id, RegistrationStatus status, CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesRegistrationStatusByIdAndStatusAsync(
            id,
            status,
            cancellationToken);

        return NoContent();
    }

    #endregion RegistrationStatus

    #region PublicationLevel

    /// <summary>
    /// Updates the publication level proposal of a public service.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="proposal">The new publication level proposal or an empty value to reset an existing one.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/publicationLevelProposal")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdatePublicationLevelProposal([FromRoute] Guid id, PublicationLevel? proposal, CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesPublicationLevelProposalByIdAndProposalAsync(
            id,
            proposal,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level of a public service.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="level">The new publication level.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}/publicationLevel")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<ActionResult> UpdatePublicationLevel([FromRoute] Guid id, PublicationLevel level, CancellationToken cancellationToken)
    {
        await _apiClient.PutPublicServicesPublicationLevelByIdAndLevelAsync(
            id,
            level,
            cancellationToken);

        return NoContent();
    }

    #endregion PublicationLevel

    #region Relation IsDescribedAt

    /// <summary>
    /// Returns datasets describing the public service.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of describing datasets.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/isDescribedAt")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<Dataset>))]
    [BadRequest]
    [AllowAnonymous]
    public async Task<IEnumerable<Dataset>> GetIsDescribedAtDatasetsByPublicServiceId(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetPublicServicesIsDescribedAtByIdAsync(id, cancellationToken);
        
        return _mapper.Map<IEnumerable<Dataset>>(response.Result);
    }

    #endregion Relation IsDescribedAt

    #region Relation Relation

    /// <summary>
    /// Returns related public services.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of related public services.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/relation")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<PublicServiceView>))]
    [BadRequest]
    [AllowAnonymous]
    public async Task<IEnumerable<PublicServiceView>> GetRelationPublicServicesByPublicServiceId(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetPublicServicesRelationsByIdAsync(id, cancellationToken);
        return _mapper.Map<IEnumerable<PublicServiceView>>(response.Result);
    }

    #endregion Relation Relation

    #region Relation Requires

    /// <summary>
    /// Returns required public services.
    /// </summary>
    /// <param name="id">The public service id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of requires public services .</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/requires")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<PublicServiceView>))]
    [BadRequest]
    [Unauthorized]
    public async Task<IEnumerable<PublicServiceView>> GetRequiresPublicServicesByPublicServiceId(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetPublicServicesRequiresByIdAsync(id, cancellationToken);
        return _mapper.Map<IEnumerable<PublicServiceView>>(response.Result);
    }

    #endregion Relation Requires
}