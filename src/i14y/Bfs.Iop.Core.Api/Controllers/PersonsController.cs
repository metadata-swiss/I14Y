using Bfs.Iop.Core.Abstractions.Commands.IopPersons;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Api.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PersonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Searches for persons in the iop persons list
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="page" example="1"></param>
    /// <param name="pageSize" example="25"></param>
    /// <param name="cancellationToken"></param>
    [HttpGet("search/{query}")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<IopPersonModel>))]
    [BadRequest]
    [Unauthorized]
    [InternalServerError]
    [Authorize]
    public async Task<IEnumerable<IopPersonModel>> SearchIopPersons(
        [FromRoute] string query,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var command = new SearchIopPersonsByQueryCommand(query, page, pageSize);
        var result = await _mediator.Send(command, cancellationToken);

        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);
        return result.Results;
    }

    /// <summary>
    /// Self register as Iop person (token needed)
    /// </summary>
    /// <param name="cancellationToken"></param>
    [HttpGet("self-registered")]
    [NoContent]
    [BadRequest]
    [InternalServerError]
    [Unauthorized]
    [Forbidden]
    public async Task<IActionResult> SelfRegister(CancellationToken cancellationToken)
    {
        var command = new AddOrUpdateCurrentUserCommand();
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Retrieves Iop Person by email.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{email}")]
    [ProducesJson]
    [Authorize]
    [BadRequest]
    [Unauthorized]
    [InternalServerError]
    [Ok(typeof(IopPersonModel))]
    public Task<IopPersonModel> GetIopPerson(
        [FromRoute] string email,
        CancellationToken cancellationToken)
    {
        var command = new GetIopPersonByEmailCommand(email);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Post Iop Persons
    /// </summary>
    /// <param name="iopPersons">List of Iop Persons to be posted.</param>
    /// <param name="cancellationToken"></param>
    [HttpPost]
    [BadRequest]
    [InternalServerError]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [Conflict]
    [Created]
    public async Task<IActionResult> PostIopPersons(
        [FromBody] IEnumerable<IopPersonModel> iopPersons,
        CancellationToken cancellationToken)
    {
        var command = new SeedIopPersonsCommand(iopPersons);
        await _mediator.Send(command, cancellationToken);

        return Created();
    }
}
