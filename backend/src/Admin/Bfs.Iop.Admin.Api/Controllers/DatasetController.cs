using Bfs.Iop.Admin.Commands.DatasetView;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Serialization.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The dataset controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DatasetController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="DatasetController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public DatasetController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Gets the dataset by id
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}")]
    [ProducesJson]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(Dataset))]
    public Task<Dataset> GetDataset(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetByIdCommand(id), cancellationToken);

    /// <summary>
    /// Gets the (authorized) data services serving the dataset.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/is-served-by")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(IEnumerable<DataServiceModel>))]
    public async Task<IEnumerable<DataServiceModel>> GetDataServicesServingDcatDataset(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetDatasetsIsServedByByIdAndPageAndPageSizeAsync(
            id, 
            page: 1, 
            pageSize: int.MaxValue, 
            cancellationToken);
        
        return response.Result;
    }

    /// <summary>
    /// Returns all distributions for the dataset
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/distributions")]
    [ProducesJson]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<DistributionSummary>))]
    public Task<IEnumerable<DistributionSummary>> GetDistributions(Guid id, CancellationToken cancellationToken)
    {
        var command = new Commands.DistributionSummary.GetAllByDatasetIdCommand(id);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Gets the registration status of the dataset.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/registrationStatus")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(RegistrationStatusInfoModel))]
    public async Task<RegistrationStatusInfoModel> GetRegistrationStatus(Guid id, CancellationToken cancellationToken) =>
        (await _apiClient.GetDatasetsRegistrationStatusByIdAsync(id, cancellationToken)).Result;

    /// <summary>
    /// Gets the publication level of the dataset.
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/publicationLevel")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Ok(typeof(PublicationLevelInfoModel))]
    public async Task<PublicationLevelInfoModel> GetPublicationLevel(Guid id, CancellationToken cancellationToken) =>
        (await _apiClient.GetDatasetsPublicationLevelByIdAsync(id, cancellationToken)).Result;

    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}/export/{format}")]
    [AllowAnonymous]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<ActionResult> ExportDataset(Guid id, [FromRoute]DataFormat format, CancellationToken cancellationToken)
    {
        if (format is not DataFormat.Json)
        {
            throw new NotSupportedException($"The format '{format}' is not supported.");
        }

        var response = await _apiClient.GetDatasetsByIdAsync(id, cancellationToken);

        var fileName = $"Dataset_{response.Result.Identifiers.First()}";
        var contentType = "application/json" ;

        var file = IopJsonSerializer.SerializeToFile(fileName, response.Result);

        return File(file.Data, contentType, file.FileName);
    }
}