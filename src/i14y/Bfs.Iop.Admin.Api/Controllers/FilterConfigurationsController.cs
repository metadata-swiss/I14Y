using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FilterConfigurationsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public FilterConfigurationsController(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Returns the Filter Configuration for the given object.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [EnableCors("AllowBIT")]
    [HttpGet]
    [Route("{id:guid}")]
    [AllowAnonymous]
    [BadRequest]
    [NoContent]
    [InternalServerError]
    [Ok(typeof(FilterConfigurationModel))]
    public async Task<ActionResult<FilterConfigurationModel>> GetFilterConfiguration(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetFilterConfigurationsByIdAsync(id, cancellationToken);
            return Ok(response.Result);
        }
        catch (ApiException ex) when (ex.StatusCode == StatusCodes.Status404NotFound)
        {
            return NoContent();
        }
    }
}
