using Bfs.Iop.Admin.Commands.VocabularyEntryView;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The vocabulary controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VocabularyController : Controller
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="VocabularyController"/> instance
    /// </summary>
    public VocabularyController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Retrieves all vocabulary entries of the specified vocabulary
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpGet("{identifier}")]
    [ProducesJson]
    [NotFound]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<VocabularyEntry>))]
    public async Task<ActionResult<IEnumerable<VocabularyEntry>>> GetVocabulary(string identifier, CancellationToken cancellationToken)
    {
        var command = new GetAllByIdentifierCommand(identifier);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Returns all the vocabulary configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all vocabulary configurations.</returns>
    [EnableCors("AllowBIT")]
    [HttpGet("configurations")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<VocabularyConfigModel>))]
    [AllowAnonymous]
    public async Task<IEnumerable<VocabularyConfigModel>> GetVocabularyConfigs(CancellationToken cancellationToken) =>
        (await _apiClient.GetVocabulariesConfigurationsAsync(cancellationToken)).Result;
}
