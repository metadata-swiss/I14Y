using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabulariesController : Controller
{
    private readonly IMediator _mediator;

    public VocabulariesController(IMediator mediator) =>
        _mediator = mediator;

    /// <summary>
    /// Returns all the vocabulary configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all vocabulary configurations.</returns>
    [HttpGet("configurations")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<VocabularyConfigModel>))]
    [AllowAnonymous]
    public Task<IEnumerable<VocabularyConfigModel>> GetVocabularyConfigs(CancellationToken cancellationToken)
    {
        var command = new GetVocabularyConfigsCommand();
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Returns a vocabulary configuration by ID.
    /// </summary>
    /// <param name="id">The vocabulary configuration ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The vocabulary configuration.</returns>
    [HttpGet("configurations/{id}")]
    [ProducesJson]
    [Ok(typeof(VocabularyConfigModel))]
    [NotFound]
    [AllowAnonymous]
    public Task<VocabularyConfigModel> GetVocabularyConfig(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetVocabularyConfigCommand(id);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Retrieves all vocabulary entries of the specified vocabulary.
    /// </summary>
    /// <param name="identifier">The vocabulary identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A vocabulary.</returns>
    [HttpGet("{identifier}")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Ok(typeof(VocabularyModel))]
    public Task<VocabularyModel> GetVocabulary(
        string identifier, 
        CancellationToken cancellationToken)
    {
        var command = new GetVocabularyCommand(identifier);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Creates a new vocabulary configuration.
    /// </summary>
    /// <param name="model">The vocabulary configuration model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("configurations")]
    [Authorize]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Conflict]
    [Created]
    public async Task<ActionResult<Guid>> PostVocabularyConfig(VocabularyConfigInputModel model, CancellationToken cancellationToken)
    {
        var command = new CreateVocabularyConfigCommand(model);
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetVocabularyConfig),
            new { id = result },
            result);
    }

    /// <summary>
    /// Updates an existing vocabulary configuration.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model">The vocabulary configuration model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPut("configurations/{id:guid}")]
    [Authorize]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Conflict]
    [NoContent]
    public async Task<ActionResult> PutVocabularyConfig(Guid id, VocabularyConfigInputModel model, CancellationToken cancellationToken)
    {
        var command = new UpdateVocabularyConfigCommand(id, model);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a vocabulary configuration.
    /// </summary>
    /// <param name="id">The ID of the vocabulary configuration to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("configurations/{id}")]
    [Authorize]
    [ProducesJson]
    [NotFound]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    public async Task<ActionResult> DeleteVocabularyConfig(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteVocabularyConfigCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}