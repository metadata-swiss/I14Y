using Bfs.Iop.Admin.Commands.Geocat.Search;
using Bfs.Iop.Admin.Commands.OpenData.Search;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Common.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetasearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetasearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search the geocat catalog
    /// </summary>
    /// <param name="language">The language to use for the search</param>
    /// <param name="query">The search query</param>
    /// <param name="page">The number of the result page to return</param>
    /// <param name="pageSize">The size of each result page</param>
    /// <returns></returns>
    [HttpGet]
    [Route("geocat/search")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<MetaSearchResultItem>))]
    [BadRequest]
    public async Task<IEnumerable<MetaSearchResultItem>> GeocatSearch([BindRequired, FromQuery] string language, [FromQuery] string? query, [BindRequired, FromQuery] int page, [BindRequired, FromQuery] int pageSize)
    {
        var response = await _mediator.Send(
            new GeocatSearchCommand
            {
                Culture = language,
                Query = query,
                Page = page,
                PageSize = pageSize
            });

        HttpContext.AddPagingHeaders(response.Page, response.PageSize, response.TotalCount);

        return response.Results;
    }

    /// <summary>
    /// Search the open data catalog
    /// </summary>
    /// <param name="language">The language to use for the search</param>
    /// <param name="query">The search query</param>
    /// <param name="page">The number of the result page to return</param>
    /// <param name="pageSize">The size of each result page</param>
    /// <returns></returns>
    [HttpGet]
    [Route("opendata/search")]
    [ProducesJson]
    [Ok(typeof(IEnumerable<MetaSearchResultItem>))]
    [BadRequest]
    public async Task<IEnumerable<MetaSearchResultItem>> OpenDataSearch([BindRequired, FromQuery] string language, [FromQuery] string? query, [BindRequired, FromQuery] int page, [BindRequired, FromQuery] int pageSize)
    {
        var response = await _mediator.Send(
            new OpenDataSearchCommand
            {
                Culture = language,
                Query = query,
                Page = page,
                PageSize = pageSize
            });

        HttpContext.AddPagingHeaders(response.Page, response.PageSize, response.TotalCount);

        return response.Results;
    }
}