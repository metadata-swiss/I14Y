using Bfs.Iop.Admin.Commands.IopPerson;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[Route("api/[controller]")]
[ApiController]

public class PersonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Searches for persons in the iop persons list
    /// </summary>
    /// <param name="query">the search query</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpGet("{query}")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<ActiveDirectoryUser>))]
    [BadRequest]
    [InternalServerError]
    [Authorize]
    public async Task<IActionResult> GetIopPersons([FromRoute] string query, CancellationToken cancellationToken)
    {
        var command = new SearchIopPersonsByQueryCommand(query);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}