using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DatasetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DatasetsController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets the DCAT dataset with the given id.
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
    [Ok(typeof(DcatDatasetModel))]
    public Task<DcatDatasetModel> GetDcatDataset(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetDatasetCommand(id), cancellationToken);

    /// <summary>
    /// Gets the DCAT dataset with the given identifier.
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
    [Ok(typeof(DcatDatasetModel))]
    public Task<DcatDatasetModel> GetDcatDatasetByIdentifier(string identifier, CancellationToken cancellationToken) =>
        _mediator.Send(new GetDatasetByIdentifierCommand(identifier), cancellationToken);

    /// <summary>
    /// Gets the (authorized) data services serving the dataset.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetDataServicesServingDatasetCommand(
                id,
                page,
                pageSize),
            cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets the (authorized) data services set as access services in a distribution.
    /// </summary>
    /// <param name="datasetId"></param>
    /// <param name="distributionId"></param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{datasetId:guid}/distributions/{distributionId:guid}/access-services")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [Forbidden]
    [Unauthorized]
    [Ok(typeof(IEnumerable<DataServiceModel>))]
    public async Task<IEnumerable<DataServiceModel>> GetDataServicesFromDistributionAccessServices(
        Guid datasetId,
        Guid distributionId,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetDatasetDistributionAccessServicesCommand(
                datasetId,
                distributionId,
                page,
                pageSize),
            cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets the DCAT datasets matching the given filters.
    /// </summary>
    /// <param name="accessRights">Code from RightsStatement_ACCESS_RIGHTS vocabulary.</param>
    /// <param name="datasetIdentifier"></param>
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
    [Ok(typeof(IEnumerable<DcatDatasetModel>))]
    public async Task<IEnumerable<DcatDatasetModel>> GetDcatDatasets(
        string? accessRights,
        string? datasetIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetDatasetsCommand(
                accessRights,
                datasetIdentifier,
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
    /// Gets the publication level information of a specific dataset.
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
        _mediator.Send(new GetPublicationLevelInfoCommand(PublishableType.Dataset, id), cancellationToken);

    /// <summary>
    /// Gets the registration status information of a specific dataset.
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
        _mediator.Send(new GetRegistrationStatusInfoCommand(PublishableType.Dataset, id), cancellationToken);

    /// <summary>
    /// Gets the next versions from a specific dataset.
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
    [Ok(typeof(IEnumerable<DcatDatasetModel>))]
    public async Task<IEnumerable<DcatDatasetModel>> GetNextVersions(
        Guid id,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDatasetNextVersionsCommand(id, page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Creates a new DCAT dataset.
    /// </summary>
    /// <param name="datasetInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Created]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [Authorize]
    public async Task<ActionResult<Guid>> PostDcatDataset(DcatDatasetInputModel datasetInput, CancellationToken cancellationToken)
    {
        var datasetId = await _mediator.Send(new CreateDatasetCommand(datasetInput), cancellationToken);
        return CreatedAtAction(nameof(GetDcatDataset), new { id = datasetId }, datasetId);
    }

    /// <summary>
    /// Uploads a new file for the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="importFile">The file to be imported.</param>
    /// <param name="cancellationToken"></param>
    [HttpPost]
    [Route("{id:guid}/model/import")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [Conflict]
    [NoContent]
    public async Task<IActionResult> UploadModel(
        Guid id,
        IFormFile importFile,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ImportDatasetModelCommand(id, importFile), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Downloads the file for the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="format">Export file format</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Route("{id:guid}/model/export/{format}")]
    [AllowAnonymous]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Produces(typeof(FileStreamResult))]
    public async Task<FileStreamResult> ExportModel(
        Guid id,
        [FromRoute][Required] LinkedDataFormat format,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExportDatasetModelCommand(id, format), cancellationToken);
        return File(result.Data, result.MimeType, result.FileName);
    }

    /// <summary>
    /// Checks whether file exists for the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A boolean value indicating whether the file exists (true) or not (false).
    /// </returns>
    [HttpGet]
    [Route("{id:guid}/model/exists")]
    [Ok(typeof(bool))]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    public async Task<bool> ModelExists(
       Guid id,
       CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckDatasetModelExistCommand(id), cancellationToken);
        return result;
    }

    /// <summary>
    /// Deletes the file associated with the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="cancellationToken"></param>
    [HttpDelete]
    [Route("{id:guid}/model/delete")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> DeleteModel(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDatasetModelCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Return dataset model in format schemaGraph.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Route("{id:guid}/model/graph")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Ok(typeof(SchemaGraph))]
    public Task<SchemaGraph> GetModelGraph(
        Guid id,
        CancellationToken cancellationToken) =>
        _mediator.Send(new GetDatasetModelGraphCommand(id), cancellationToken);

    /// <summary>
    /// Updates the class position of a dataset
    /// </summary>
    /// <param name="id"></param>
    /// <param name="classesPositionInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/model/position")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutDatasetModelPosition(
        Guid id,
        [Required] Dictionary<string, SchemaPoint> classesPositionInput,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateDatasetModelPositionCommand(id, classesPositionInput),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates an attribute of a schemaclass in the structure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="schemaClassInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/model/class")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutDatasetModelClass(
        Guid id,
        SchemaClass schemaClassInput,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateDatasetModelClassCommand(id, schemaClassInput), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates an attribute of a PropertyShape in the structure of the dataset with the given id,
    /// attached to the class identified by <c>classUri</c> in the request body.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="classUri"></param>
    /// <param name="propertyInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/model/property")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutDatasetModelProperty(
        Guid id,
        [Required] Uri classUri,
        [Required] SchemaProperty propertyInput,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateDatasetModelPropertyCommand(id, propertyInput, classUri),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Creates a new schemaclass in the structure of the dataset with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="schemaClassInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The URI of the newly created class.</returns>
    [HttpPost]
    [Route("{id:guid}/model/class")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Created]
    public async Task<ActionResult<Uri>> PostDatasetModelClass(
        Guid id,
        [Required] SchemaClass schemaClassInput,
        CancellationToken cancellationToken)
    {
        var newClassUri = await _mediator.Send(
            new CreateDatasetModelClassCommand(id, schemaClassInput), cancellationToken);

        return CreatedAtAction(nameof(GetModelGraph), new { id }, newClassUri);
    }

    /// <summary>
    /// Creates a new PropertyShape in the structure of the dataset with the given id,
    /// attached to the class identified by <c>ClassUri</c> in the request body.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="classUri"></param>
    /// <param name="propertyInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The URI of the newly created property.</returns>
    [HttpPost]
    [Route("{id:guid}/model/property")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NotFound]
    [Created]
    public async Task<ActionResult<Uri>> PostDatasetModelProperty(
        Guid id,
        [Required] Uri classUri,
        [Required] SchemaProperty propertyInput,
        CancellationToken cancellationToken)
    {
        var newPropertyUri = await _mediator.Send(
            new CreateDatasetModelPropertyCommand(id, propertyInput, classUri),
            cancellationToken);

        return CreatedAtAction(nameof(GetModelGraph), new { id }, newPropertyUri);
    }

    /// <summary>
    /// Updates the publication level of the dataset with the given id.
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
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevel(
        Guid id,
        [FromQuery][Required] PublicationLevel level,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelCommand(PublishableType.Dataset, id, level),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level proposal of the dataset with the given id.
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
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutPublicationLevelProposal(
        Guid id,
        [FromQuery] PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdatePublicationLevelProposalCommand(PublishableType.Dataset, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of the dataset with the given id.
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
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatus(
        Guid id,
        [FromQuery][Required] RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusCommand(PublishableType.Dataset, id, status),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration status proposal of the dataset with the given id.
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
    [NotFound]
    [NoContent]
    public async Task<IActionResult> PutRegistrationStatusProposal(
        Guid id,
        [FromQuery] RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateRegistrationStatusProposalCommand(PublishableType.Dataset, id, proposal),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates an existing dataset with the specified id.
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
    public async Task<IActionResult> PutDataset(
        Guid id,
        DcatDatasetInputModel inputModel,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDatasetCommand(id, inputModel);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing dataset with the specified id.
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
    public async Task<IActionResult> DeleteDataset(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDatasetCommand(id);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}