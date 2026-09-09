using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatasetQualityInformationController : ControllerBase
{
    private readonly IMediator _mediator;

    public DatasetQualityInformationController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Creates a new set of quality informations for a given dataset.
    /// </summary>
    /// <param name="qualityInformations">The set of quality informations </param>
    /// <param name="cancellationToken"></param>
    [HttpPost]
    [EnableCors("AllowBIT")]
    [ProducesJson]
    [Ok(typeof(DatasetQualityInformationData))]
    [Created]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [Authorize]
    public async Task<ActionResult<DatasetQualityInformationData>> CreateQualityInformations(DatasetQualityInformationData qualityInformations, CancellationToken cancellationToken)
    {
        var command = new CreateCommand(qualityInformations);
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(CreateQualityInformations), new { id = result.DatasetId }, result);
    }

    /// <summary>
    /// Deletes from a dataset all quality informaitons.
    /// </summary>
    /// <param name="datasetId">The id of the dataset to remove all quality informations from.</param>
    /// <param name="cancellationToken"></param>
    [HttpDelete("{datasetId:guid}")]
    [EnableCors("AllowBIT")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    [InternalServerError]
    [Authorize]
    public async Task<ActionResult> DeleteQualityInformations(Guid datasetId, CancellationToken cancellationToken)
    {
        var command = new DeleteByDatasetIdCommand(datasetId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Lists all questions.
    /// </summary>
    [HttpGet("definition")]
    [EnableCors("AllowBIT")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<DatasetQualityQuestion>))]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DatasetQualityQuestion>>> GetAllQuestions(CancellationToken cancellationToken)
    {
        var command = new GetAllCommand();
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lists quality informations by a dataset
    /// </summary>
    [HttpGet("{datasetId:guid}")]
    [EnableCors("AllowBIT")]
    [ProducesJson]
    [Ok(typeof(DatasetQualityInformationData))]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [AllowAnonymous]
    public async Task<ActionResult<DatasetQualityInformationData>> GetQualityInformationsByDataset(Guid datasetId, CancellationToken cancellationToken)
    {
        var command = new GetByDatasetIdCommand(datasetId);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Upddate a set of quality informations for a given dataset.
    /// </summary>
    /// <param name="qualityInformations">The set of quality informations </param>
    /// <param name="cancellationToken"></param>
    [HttpPut]
    [EnableCors("AllowBIT")]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    [InternalServerError]
    [Authorize]
    public async Task<ActionResult> UpdateQualityInformations(DatasetQualityInformationData qualityInformations, CancellationToken cancellationToken)
    {
        var command = new UpdateCommand(qualityInformations);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}