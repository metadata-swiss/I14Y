using Bfs.Iop.Admin.Commands.Geocat.Search;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.Geocat;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.Geocat;

public sealed class GeocatSearchHandler : IRequestHandler<GeocatSearchCommand, PagedResult<MetaSearchResultItem>>
{
    private readonly IGeocatIndex _index;
    private readonly IMapper _mapper;

    public GeocatSearchHandler(IGeocatIndex index, IMapper mapper)
    {
        _index = index ?? throw new ArgumentNullException(nameof(index));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<MetaSearchResultItem>> Handle(GeocatSearchCommand request, CancellationToken cancellationToken)
    {
        var result = await _index.Search(request.Query, request.Culture, request.Page, request.PageSize, cancellationToken);

        using var scope = new MapContextScope();

        MapContext.Current.Parameters["language"] = request.Culture;
        MapContext.Current.Parameters["pageSize"] = request.PageSize;

        return result.Adapt<PagedResult<MetaSearchResultItem>>();
    }
}