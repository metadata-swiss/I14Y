using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DcatCatalogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DcatCatalogsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Gets the DCAT catalog with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DcatCatalogModel))]
    public Task<DcatCatalogModel> GetDcatCatalog(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetDcatCatalogCommand(id), cancellationToken);

    /// <summary>
    /// Gets all the themes from a specific DCAT catalog.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/themes")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(IEnumerable<DcatCatalogThemeModel>))]
    public async Task<IEnumerable<DcatCatalogThemeModel>> GetDcatCatalogThemes(
        Guid id,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var command = new GetDcatCatalogThemesCommand(id, page, pageSize);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets all the DCAT catalogs.
    /// </summary>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<DcatCatalogModel>))]
    public async Task<IEnumerable<DcatCatalogModel>> GetDcatCatalogs(int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDcatCatalogsCommand(page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets all the DCAT catalogs from the current user agents.
    /// </summary>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("user-agents")]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<DcatCatalogModel>))]
    public async Task<IEnumerable<DcatCatalogModel>> GetDcatCatalogsFromCurrentUserAgents(int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDcatCatalogsFromCurrentUserAgentsCommand(page, pageSize), cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Creates a new Dcat catalog.
    /// </summary>
    /// <param name="inputModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostDcatCatalog(DcatCatalogInputModel inputModel, CancellationToken cancellationToken)
    {
        var command = new CreateDcatCatalogCommand(inputModel);
        var guid = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetDcatCatalog), new { id = guid }, guid);
    }

    /// <summary>
    /// Updates an existing DCAT catalog.
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
    public async Task<IActionResult> PutDcatCatalog(Guid id, DcatCatalogInputModel updateModel, CancellationToken cancellationToken)
    {
        var command = new UpdateDcatCatalogCommand(id, updateModel);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing DCAT catalog with the specified id.
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
    public async Task<IActionResult> DeleteDcatCatalog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDcatCatalogCommand(id);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets all the records from a specific DCAT catalog.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/records")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(IEnumerable<DcatCatalogRecordModel>))]
    public async Task<IEnumerable<DcatCatalogRecordModel>> GetDcatCatalogRecords(
        Guid id, 
        int? page,
        int? pageSize, 
        CancellationToken cancellationToken)
    {
        var command = new GetDcatCatalogRecordsCommand(id, page, pageSize);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets all the records from a specific resource.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("records/from-resource/{resourceId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(IEnumerable<DcatCatalogRecordModel>))]
    public async Task<IEnumerable<DcatCatalogRecordModel>> GetDcatCatalogRecordsFromResource(
        Guid resourceId,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var command = new GetDcatCatalogRecordsFromResourceCommand(resourceId, page, pageSize);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Gets a specific DCAT catalog record.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="recordId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id:guid}/records/{recordId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DcatCatalogRecordModel))]
    public Task<DcatCatalogRecordModel> GetDcatCatalogRecord(Guid id, Guid recordId, CancellationToken cancellationToken)
    {
        var command = new GetDcatCatalogRecordCommand(id, recordId);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Creates new Dcat catalog records.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inputModels"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{id:guid}/records")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<IEnumerable<Guid>>> PostDcatCatalogRecords(Guid id, IEnumerable<DcatCatalogRecordInputModel> inputModels, CancellationToken cancellationToken)
    {
        var command = new CreateDcatCatalogRecordsCommand(id, inputModels);
        var guids = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, guids);
    }

    /// <summary>
    /// Updates an existing DCAT catalog record.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="recordId"></param>
    /// <param name="updateModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{id:guid}/records/{recordId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> PutDcatCatalogRecord(Guid id, Guid recordId, DcatCatalogRecordInputModel updateModel, CancellationToken cancellationToken)
    {
        var command = new UpdateDcatCatalogRecordCommand(id, recordId, updateModel);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing DCAT catalog record with the specified ids.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="recordId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{id:guid}/records/{recordId:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteDcatCatalogRecord(
        Guid id,
        Guid recordId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDcatCatalogRecordCommand(id, recordId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Exports the content of a DcatCatalog in a standardized format.
    /// </summary>
    /// <param name="id">Id of the DcatCatalog to be exported.</param>
    /// <param name="dataFormat">Selection of standardized formats.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Route("{id:Guid}/export/{dataFormat}")]
    [BadRequest]
    [InternalServerError]
    [AllowAnonymous]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDcatCatalogById(
        Guid id,
        RdfExportFormat dataFormat,
        CancellationToken cancellationToken)
    {
        var mimeType = dataFormat switch
        {
            RdfExportFormat.TTL => "application/x-turtle",
            RdfExportFormat.RDF => "application/rdf+xml",
            _ => throw new NotSupportedException($"The format '{dataFormat}' is not supported.")
        };

        var cmd = new ExportDcatCatalogCommand(id, dataFormat);
        var result = await _mediator.Send(cmd, cancellationToken);
        return Content(result, mimeType);
    }
}
