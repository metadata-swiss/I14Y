using Bfs.Iop.Admin.Commands.DcatCatalog;
using Bfs.Iop.Admin.Commands.DcatCatalogRecords;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
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
public class DcatCatalogInputController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a <see cref="DcatCatalogInputController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    public DcatCatalogInputController(IMediator mediator)
        => _mediator = mediator;

    #region DcatCatalogRecord

    /// <summary>
    /// Deletes an existing catalog record.
    /// </summary>
    /// <param name="id">The catalog id.</param>
    /// <param name="recordId">The record id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    [EnableCors("AllowBIT")]
    [HttpDelete("{id:guid}/records/{recordId:guid}")]
    [ProducesJson]
    [NotFound]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [InternalServerError]
    public async Task<ActionResult> DeleteDcatCatalogRecord(Guid id, Guid recordId, CancellationToken cancellationToken)
    {
        var command = new DeleteDcatCatalogRecordCommand(id, recordId);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Creates a new Dcat CatalogRecordInput based on Dcat Catalog.
    /// </summary>
    /// <param name="model">The CatalogRecordInput.</param>
    /// <returns>The Dcat CatalogRecordInput created.</returns>
    [EnableCors("AllowBIT")]
    [HttpPost("records")]
    [ProducesJson]
    [Ok(typeof(DcatCatalogRecordInput))]
    [NotFound]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [InternalServerError]
    public async Task<ActionResult<DcatCatalogRecordInput>> PostDcatCatalogRecord(DcatCatalogRecordInput model, CancellationToken cancellationToken)
    {
        var command = new AddDcatCatalogRecordCommand(model);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing Dcat CatalogRecordInput based on the body content.
    /// </summary>
    /// <param name="model">The Dcat CatalogRecordInput to be modified, which is transferred via body content in json format.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No Content.</returns>
    [EnableCors("AllowBIT")]
    [HttpPut("records")]
    [ProducesJson]
    [NotFound]
    [BadRequest]
    [Unauthorized]
    [Forbidden]
    [NoContent]
    [InternalServerError]
    public async Task<ActionResult> PutDcatCatalogRecord(DcatCatalogRecordInput model, CancellationToken cancellationToken)
    {
        var command = new Commands.DcatCatalogRecords.UpdateDcatCatalogRecordCommand(model);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    #endregion DcatCatalogRecord

    /// <summary>
    /// List all Dcat Catalogs filtered by user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    ///<returns>A collection of Dcat Catalogs.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("user")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<Models.DcatCatalog>))]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Models.DcatCatalog>>> GetAllDcatCatalogs(CancellationToken cancellationToken)
    {
        var command = new GetAllDcatCatalogsByUserCommand();
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// List all Themes from Dcat Catalog.
    /// </summary>
    /// <param name="id">The Dcat Catalog id</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of VocabularyEntry.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("{id:guid}/themes")]
    [ProducesJson]
    [BadRequest]
    [Ok(typeof(IEnumerable<Models.DcatVocabularyEntry>))]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Models.DcatVocabularyEntry>>> GetAllDcatCatalogThemes(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetThemesByDcatCatalogIdCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///  Returns the Dcat CatalogRecordInput with the corresponding Dcat CatalogRecordInput.PrimaryTopic.ResouceId.
    /// </summary>
    /// <param name="resourceId">The CatalogRecordInput.PrimaryTopic.ResouceId of the Dcat CatalogRecordInput to be returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [EnableCors("AllowBIT")]
    [HttpGet("records/byResource/{resourceId:guid}")]
    [ProducesJson]
    [BadRequest]
    [Ok(typeof(IEnumerable<DcatCatalogRecordInput>))]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DcatCatalogRecordInput>>> GetDcatCatalogRecordById(Guid resourceId, CancellationToken cancellationToken)
    {
        var command = new GetDcatCatalogRecordByResourceIdCommand(resourceId);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}