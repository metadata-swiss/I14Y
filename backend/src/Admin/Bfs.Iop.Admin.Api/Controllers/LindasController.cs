using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Bfs.Iop.Admin.Commands.Lindas;
using Bfs.Iop.Admin.Models.Lindas;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LindasController : ControllerBase
{
    private readonly IMediator _mediator;

    public LindasController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets a LINDAS RDF download URL for the requested resource.
    /// </summary>
    [HttpGet("rdf-link")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [NoContent]
    [InternalServerError]
    [Ok(typeof(Uri))]
    public async Task<IActionResult> GetRdfUrl(
        [Required][FromQuery] LindasResourceType type,
        [Required][FromQuery] string identifier,
        [FromQuery] string? version,
        CancellationToken cancellationToken)
    {
        var url = await _mediator.Send(
            new GetLindasRdfLinkCommand
            {
                Type = type,
                Identifier = identifier,
                Version = version
            },
            cancellationToken);

        return url is null
            ? NoContent()
            : Ok(url);
    }

    /// <summary>
    /// Gets the LINDAS URI for the requested resource when it is available on ld.admin.ch.
    /// </summary>
    [HttpGet("ld-uri")]
    [ProducesJson]
    [AllowAnonymous]
    [BadRequest]
    [NoContent]
    [InternalServerError]
    [Ok(typeof(Uri))]
    public async Task<IActionResult> GetLdUri(
        [Required][FromQuery] LindasResourceType type,
        [Required][FromQuery] string identifier,
        [FromQuery] string? version,
        CancellationToken cancellationToken)
    {
        var uri = await _mediator.Send(
            new GetLindasLdUriCommand
            {
                Type = type,
                Identifier = identifier,
                Version = version
            },
            cancellationToken);

        return uri is null
            ? NoContent()
            : Ok(uri);
    }
}