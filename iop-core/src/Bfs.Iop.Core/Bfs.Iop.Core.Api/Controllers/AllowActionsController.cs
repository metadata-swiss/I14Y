using Bfs.Iop.Core.Abstractions.Commands.AllowActions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AllowActionsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public AllowActionsController(
        IMediator mediator,
        IConfiguration configuration)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
    /// <summary>
    /// Gets the user allowed actions for a specified resource.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="resourceType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{resourceType}/{resourceId:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [InternalServerError]
    [Ok(typeof(IEnumerable<AllowActionResult>))]
    public Task<IEnumerable<AllowActionResult>> GetAllowAction(
        AllowActionResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var command = new GetAllowActionCommand(resourceType, resourceId);
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Gets the information about the resources that the user is allowed to create.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [InternalServerError]
    [Ok(typeof(IEnumerable<AllowActionResult>))]
    public Task<IEnumerable<AllowActionResult>> GetAllowCreate(
        CancellationToken cancellationToken)
    {
        var command = new GetAllowCreateCommand();
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Gets the information if the application is in read only mode.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("read-only")]
    [AllowAnonymous]
    [InternalServerError]
    [Ok(typeof(bool))]
    public Task<bool> IsApplicationReadOnly()
    {
        var isReadOnly = _configuration.GetValue<bool>("ReadOnly");

        return Task.FromResult(isReadOnly);
    }
}
