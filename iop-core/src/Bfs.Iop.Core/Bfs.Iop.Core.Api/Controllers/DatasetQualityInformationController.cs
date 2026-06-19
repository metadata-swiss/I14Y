using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
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
public class DatasetQualityInformationController : ControllerBase
{
    private readonly IMediator _mediator;

    public DatasetQualityInformationController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Creates a new set of quality information for a dataset.
    /// </summary>
    /// <param name="qualityInformation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostQualityInformation(
        DatasetQualityInformationDataModel qualityInformation,
        CancellationToken cancellationToken)
    {
        var command = new CreateDatasetQualityInformationCommand(qualityInformation.DatasetId, qualityInformation);
        await _mediator.Send(command, cancellationToken);

        var datasetId = qualityInformation.DatasetId;

        return CreatedAtAction(nameof(GetQualityInformationByDataset), new { datasetId }, datasetId);
    }

    /// <summary>
    /// Deletes all the quality information of a dataset.
    /// </summary>
    /// <param name="datasetId"></param>
    /// <param name="cancellationToken"></param>
    [HttpDelete("{datasetId:guid}")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    [InternalServerError]
    public async Task<ActionResult> DeleteQualityInformation(Guid datasetId, CancellationToken cancellationToken)
    {
        var command = new DeleteDatasetQualityInformationCommand(datasetId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Lists all questions.
    /// </summary>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("definition")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [InternalServerError]
    [Ok(typeof(IEnumerable<DatasetQualityQuestionModel>))]
    public async Task<IEnumerable<DatasetQualityQuestionModel>> GetAllQuestions(int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var command = new GetDatasetQualityQuestionsCommand(page, pageSize);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);

        return result.Results;
    }

    /// <summary>
    /// Gets the quality information of a dataset.
    /// </summary>
    [HttpGet("{datasetId:guid}")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DatasetQualityInformationDataModel))]
    public Task<DatasetQualityInformationDataModel> GetQualityInformationByDataset(Guid datasetId, CancellationToken cancellationToken)
    {
        var command = new GetDatasetQualityInformationCommand(datasetId);

        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Updates the quality information of a dataset.
    /// </summary>
    /// <param name="qualityInformation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    [InternalServerError]
    public async Task<ActionResult> PutQualityInformation(DatasetQualityInformationDataModel qualityInformation, CancellationToken cancellationToken)
    {
        var command = new UpdateDatasetQualityInformationCommand(qualityInformation.DatasetId, qualityInformation);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}