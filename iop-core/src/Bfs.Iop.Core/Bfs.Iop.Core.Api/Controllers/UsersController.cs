using Bfs.Iop.Core.Abstractions.Commands.Users;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Returns the current user information.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("current")]
    [AllowAnonymous]
    [Ok(typeof(UserModel))]
    public Task<UserModel> GetCurrentUser(CancellationToken cancellationToken)
    {
        var command = new GetCurrentUserCommand();
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Lists all agents attached to the current user.
    /// </summary>
    [HttpGet("current-agents")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<AgentModel>))]
    [InternalServerError]
    [AllowAnonymous]
    public Task<IEnumerable<AgentModel>> GetUserAgents(CancellationToken cancellationToken)
    {
        var command = new GetCurrentUserAgentsCommand();
        return _mediator.Send(command, cancellationToken);
    }
}