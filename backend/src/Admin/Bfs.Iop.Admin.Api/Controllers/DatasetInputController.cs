using Bfs.Iop.Admin.Api.Extensions;
using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Utilities;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The dataset input controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class DatasetInputController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a <see cref="DatasetInputController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="mapper"></param>
    /// <param name="apiClient"></param>
    public DatasetInputController(IMediator mediator, IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _mapper = mapper;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Checks whether the identifier is in use
    /// </summary>
    [EnableCors("AllowBIT")]
    [HttpGet("identifier/{identifier}/exists")]
    [Authorize]
    [ProducesJson]
    [BadRequest]
    [Unauthorized]
    [Ok(typeof(bool))]
    public async Task<ActionResult<bool>> GetIdentifierExists(string identifier, CancellationToken cancellationToken)
    {
        var command = new GetByDatasetIdentifierCommand(identifier);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
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
        _ = await _apiClient.DeleteDatasetsByIdAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Add a new dataset
    /// </summary>
    /// <param name="model">The new dataset</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost]
    [ProducesJson]
    [Unauthorized]
    [BadRequest]
    [Created()]
    public async Task<ActionResult> Post(DcatDatasetInputModel model, CancellationToken cancellationToken)
    {
        var result = (await _apiClient.PostDatasetsByBodyAsync(model, cancellationToken)).Result;
        return CreatedAtAction(nameof(DatasetsController.GetDataset), "Datasets", new { id = result }, result);

    }

    /// <summary>
    /// Updates the entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPut("{id:guid}")]
    [NoContent]
    [BadRequest]
    [Unauthorized]
    [NotFound]
    public async Task<ActionResult> Put([FromRoute] Guid id, DcatDatasetInputModel model, CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsByIdAndBodyAsync(id, model, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the registration status of a dataset.
    /// </summary>
    /// <param name="id">The dataset id.</param>
    /// <param name="status">The new registration status or proposal.</param>
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
        await _apiClient.PutDatasetsRegistrationStatusByIdAndStatusAsync(
            id,
            status,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the registration proposal of a dataset.
    /// </summary>
    /// <param name="id">The dataset id.</param>
    /// <param name="proposal">The new registration proposal or empty to revert an existing state.</param>
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
        await _apiClient.PutDatasetsRegistrationStatusProposalByIdAndProposalAsync(
            id,
            proposal,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level of a dataset.
    /// </summary>
    /// <param name="id">The dataset id.</param>
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
        await _apiClient.PutDatasetsPublicationLevelByIdAndLevelAsync(
            id,
            level,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the publication level proposal of a dataset.
    /// </summary>
    /// <param name="id">The dataset id.</param>
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
        await _apiClient.PutDatasetsPublicationLevelProposalByIdAndProposalAsync(
            id,
            proposal,
            cancellationToken);

        return NoContent();
    }

    [EnableCors("AllowBIT")]
    [HttpPost]
    [Route("import")]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [Conflict]
    [InternalServerError]
    [Created()]
    public async Task<ActionResult<Guid>> ImportDataset(IFormFile file, CancellationToken cancellationToken)
    {
        var wrappedData = file.OpenReadStream().DeserializeFromStream<DataWrapper<DcatDatasetInputModel>>();

        var response = await _apiClient.PostDatasetsByBodyAsync(wrappedData.Data, cancellationToken);

        return CreatedAtAction(nameof(DatasetsController.GetDataset), "Datasets", new { id = response.Result }, response.Result);
    }

    /// <summary>
    /// Uploads a new file for the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="importFile">The file to be imported.</param>
    [HttpPost]
    [Route("{id:guid}/model/import")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [Authorize]
    [Created]
    public async Task<IActionResult> UploadModel(
           Guid id,
           IFormFile importFile,
           CancellationToken cancellationToken)
    {
        using var stream = importFile.OpenReadStream();

        FileParameter fileParameter = new FileParameter(
            stream,
            importFile.FileName,
            importFile.ContentType);

        _ = await _apiClient.PostDatasetsModelImportByIdAndBodyAsync(
                id,
                fileParameter,
                cancellationToken);

        return CreatedAtAction(nameof(ExportModel), new { id }, id);
    }


    /// <summary>
    /// Downloads the file for the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <param name="format">The export linked data format</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Route("{id:guid}/model/export")]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Produces(typeof(FileStreamResult))]
    public async Task<FileStreamResult> ExportModel(
       Guid id,
       [FromQuery][Required] LinkedDataFormat format,
       CancellationToken cancellationToken)
    {
        string contentType = "application/rdf+xml";
        string fileName = $"{id}.{format.ToString().ToLowerInvariant()}";

        var fileResponse = await _apiClient.GetDatasetsModelExportByIdAndFormatAsync(id, format, cancellationToken);

        if (fileResponse.Headers.TryGetValue("Content-Type", out var contentTypes) &&
            contentTypes.Any())
        {
            contentType = contentTypes.First();
        }

        if (fileResponse.Headers.TryGetValue("Content-Disposition", out var contentDispositions) &&
            contentDispositions.Any())
        {
            var contentDisposition = contentDispositions.First();

            var match = Regex.Match(contentDisposition, @"filename=""?([^""]+)""?");
            if (match.Success)
            {
                var matchResult = match.Groups[1].Value.Split(";");
                if (matchResult.Length > 0)
                {
                    fileName = matchResult[0];
                }
            }
        }

        return new FileStreamResult(fileResponse.Stream, contentType)
        {
            FileDownloadName = fileName
        };
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
    [Unauthorized]
    [Forbidden]
    public async Task<bool> ModelExists(
       Guid id,
       CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetsModelExistsByIdAsync(id, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Deletes the file associated with the specified dataset.
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    [HttpDelete]
    [Route("{id:guid}/model/delete")]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [Authorize]
    public async Task<IActionResult> DeleteModel(
       Guid id,
       CancellationToken cancellationToken)
    {
        await _apiClient.DeleteDatasetsModelDeleteByIdAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Return dataset model in format shemaGraph.
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
    public async Task<SchemaGraph> GetModelGraph(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetsModelGraphByIdAsync(id, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Update dataset class position
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
    public async Task<IActionResult> UpdateDatasetModelPosition(
        Guid id,
        [Required] IDictionary<string, SchemaPoint> classesPositionInput,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsModelPositionByIdAndBodyAsync(id, classesPositionInput, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update dataset model class
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
    public async Task<IActionResult> UpdateDatasetModelClass(
        Guid id,
        [Required] SchemaClass schemaClassInput,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsModelClassByIdAndBodyAsync(id, schemaClassInput, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Create a new schemaclass in the dataset structure.
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
    public async Task<ActionResult<Uri>> CreateDatasetModelClass(
        Guid id,
        [Required] SchemaClass schemaClassInput,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.PostDatasetsModelClassByIdAndBodyAsync(id, schemaClassInput, cancellationToken);
        return CreatedAtAction(nameof(GetModelGraph), new { id }, response.Result);
    }

    /// <summary>
    /// Create a new PropertyShape attached to the class identified by <paramref name="classUri"/>.
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
    public async Task<ActionResult<Uri>> CreateDatasetModelProperty(
        Guid id,
        [Required] Uri classUri,
        [Required] SchemaProperty propertyInput,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.PostDatasetsModelPropertyByIdAndClassUriAndBodyAsync(id, classUri, propertyInput, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Update a PropertyShape attached to the class identified by <paramref name="classUri"/>.
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
    public async Task<IActionResult> UpdateDatasetModelProperty(
        Guid id,
        [Required] Uri classUri,
        [Required] SchemaProperty propertyInput,
        CancellationToken cancellationToken)
    {
        await _apiClient.PutDatasetsModelPropertyByIdAndClassUriAndBodyAsync(id, classUri, propertyInput, cancellationToken);
        return NoContent();
    }
}