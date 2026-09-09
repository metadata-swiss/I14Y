using Bfs.Iop.Admin.Api.Extensions;
using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MediaController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public MediaController(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

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
        var response = await _apiClient.GetMediaByUrlAsync(url, cancellationToken);

        var contentType = response.GetContentTypeFromHeader();

        return new FileStreamResult(response.Stream, contentType);
    }
}
