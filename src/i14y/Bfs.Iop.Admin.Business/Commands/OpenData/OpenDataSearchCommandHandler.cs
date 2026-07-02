using Bfs.Iop.Admin.Commands.OpenData.Search;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.OpenData;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.OpenData;

public sealed class OpenDataSearchCommandHandler : IRequestHandler<OpenDataSearchCommand, PagedResult<MetaSearchResultItem>>
{
    private readonly IOpenDataIndex _index;
    private readonly IMapper _mapper;

    public OpenDataSearchCommandHandler(IOpenDataIndex index, IMapper mapper)
    {
        _index = index ?? throw new ArgumentNullException(nameof(index));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<MetaSearchResultItem>> Handle(OpenDataSearchCommand request, CancellationToken cancellationToken)
    {
        var result = await _index.Search(request.Query, request.Culture, request.Page, request.PageSize, cancellationToken);

        using var scope = new MapContextScope();

        MapContext.Current.Parameters["language"] = request.Culture;
        MapContext.Current.Parameters["pageSize"] = request.PageSize;
        
        return result.Adapt<PagedResult<MetaSearchResultItem>>();
    }
}