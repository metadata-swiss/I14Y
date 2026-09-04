using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Admin.Commands.DataServiceInput;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
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
/// The dataset input controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataServiceInputController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a <see cref="DataServiceInputController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public DataServiceInputController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
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
    [Conflict]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteDataServicesByIdAsync(id, cancellationToken);

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
    public async Task<ActionResult> Put(DataServiceInput model, CancellationToken cancellationToken)
    {
        var command = new PutInputCommand<DataServiceInput>(model);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returns all datasets that are linked to this data service via a relationship.
    /// </summary>
    /// <param name="id">The data service id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of datasets.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/servesDatasets")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    [Ok(typeof(IEnumerable<Dataset>))]
    public async Task<IEnumerable<Dataset>> GetServesDatasets(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetDataServiceDatasetsCommand(id);
        return await _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Add a new dataService
    /// </summary>
    /// <param name="model">The new dataService</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost]
    [ProducesJson]
    [Unauthorized]
    [BadRequest]
    [Created()]
    public async Task<ActionResult> Post(DataServiceInput model, CancellationToken cancellationToken)
    {
        var command = new PostInputCommand<DataServiceInput>(model);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(DataServicesController.GetDataService), "DataServices", new { id = result.Id }, result);
    }

    #region RegistrationStatus

    /// <summary>
    /// Updates the registration status of a data service.
    /// </summary>
    /// <param name="id">The data service id.</param>
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
        await _apiClient.PutDataServicesRegistrationStatusByIdAndStatusAsync(
            id,
            status,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration proposal of a data service.
    /// </summary>
    /// <param name="id">The data service id.</param>
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
        await _apiClient.PutDataServicesRegistrationStatusProposalByIdAndProposalAsync(
            id,
            proposal,
            cancellationToken);

        return NoContent();
    }

    #endregion RegistrationStatus

    #region PublicationLevel

    /// <summary>
    /// Updates the publication level of a data service.
    /// </summary>
    /// <param name="id">The data service id.</param>
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
        await _apiClient.PutDataServicesPublicationLevelByIdAndLevelAsync(
            id,
            level,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level proposal of a data service.
    /// </summary>
    /// <param name="id">The data service id.</param>
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
        await _apiClient.PutDataServicesPublicationLevelProposalByIdAndProposalAsync(
            id,
            proposal,
            cancellationToken);

        return NoContent();
    }

    #endregion PublicationLevel
}