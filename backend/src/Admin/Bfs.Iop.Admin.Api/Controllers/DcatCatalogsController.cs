using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DcatCatalogsController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public DcatCatalogsController(IIopCoreApiClient apiClient) => _apiClient = apiClient;

    /// <summary>
    /// Gets a DCAT catalog by id.
    /// </summary>
    /// <param name="id">The DCAT catalog id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The DCAT catalog.</returns>
    [HttpGet("{id:guid}")]
    [ProducesJson]
    [BadRequest]
    [NotFound]
    [InternalServerError]
    [Ok(typeof(DcatCatalogModel))]
    [AllowAnonymous]
    public async Task<DcatCatalogModel> GetDcatCatalog(Guid id, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsByIdAsync(id, cancellationToken);

        return response.Result;
    }
}
