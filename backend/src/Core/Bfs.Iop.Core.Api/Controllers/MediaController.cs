using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.Abstractions.Commands.Media;
using Bfs.Iop.Core.Abstractions.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator) => _mediator = mediator 
        ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{*url}")]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(FileStreamResult))]
    public async Task<FileStreamResult> GetMedia([FromRoute] string url, CancellationToken cancellationToken)
    { 
        var command = new GetMediaCommand(url);

        var result = await _mediator.Send(command, cancellationToken);

        return new FileStreamResult(result.Data, result.MimeType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesJson]
    [InternalServerError]
    [Ok(typeof(IEnumerable<MediaInfoModel>))]
    public Task<IEnumerable<MediaInfoModel>> GetMediaInfos(CancellationToken cancellationToken)
    {
        var command = new GetMediaInfosCommand();
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [Consumes("multipart/form-data")]
    [ProducesJson]
    [Ok(typeof(MediaInfoModel))]
    public Task<MediaInfoModel> PostMedia(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var command = new CreateMediaCommand(file);

        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{*url}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [NoContent]
    public async Task<ActionResult> DeleteMedia([FromRoute] string url, CancellationToken cancellationToken)
    {
        var command = new DeleteMediaCommand(url);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
