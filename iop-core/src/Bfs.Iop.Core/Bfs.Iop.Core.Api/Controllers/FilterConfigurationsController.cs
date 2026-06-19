using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilterConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilterConfigurationsController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [InternalServerError]
    [Ok(typeof(FilterConfigurationModel))]
    public Task<FilterConfigurationModel> GetFilterConfiguration(Guid id, CancellationToken cancellationToken = default) 
        => _mediator.Send(new GetFilterConfigurationCommand(id), cancellationToken);

    [HttpPost]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [Created]
    public async Task<ActionResult<Guid>> PostFilterConfiguration(Guid id, IFormFile file, CancellationToken cancellationToken = default)
    {
        using var stream = file.OpenReadStream();

        await _mediator.Send(new CreateFilterConfigurationCommand(id, stream), cancellationToken);

        return CreatedAtAction(nameof(GetFilterConfiguration), new { id }, id);
    }

    [HttpDelete]
    [Route("{id:guid}")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [BadRequest]
    [InternalServerError]
    [NoContent]
    public async Task<IActionResult> DeleteFilterConfiguration(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteFilterConfigurationCommand(id), cancellationToken);

        return NoContent();
    }
}
