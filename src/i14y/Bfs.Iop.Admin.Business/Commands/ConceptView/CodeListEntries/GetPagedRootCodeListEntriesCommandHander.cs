using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Extensions;
using MapsterMapper;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView.CodeListEntries;

internal sealed class GetPagedRootCodeListEntriesCommandHander : 
    IRequestHandler<GetPagedRootCodeListEntriesCommand, PagedResult<CodeListEntryDetail>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetPagedRootCodeListEntriesCommandHander(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<PagedResult<CodeListEntryDetail>> Handle(GetPagedRootCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        var getCodeListEntriesByRootResponse = await _apiClient.GetConceptsCodelistEntriesByRootByIdAndSortPropertyAndSortOrderAndPageAndPageSizeAsync(
            request.ConceptId,
            request.SortProperty,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var codeListEntries = getCodeListEntriesByRootResponse.Result.ToList();

        int pageHeader = getCodeListEntriesByRootResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = getCodeListEntriesByRootResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = getCodeListEntriesByRootResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        var results = codeListEntries.Select(x =>
        {
            var detail = _mapper.Map<CodeListEntryDetail>(x);

            var hasChildren = _apiClient.GetConceptsCodelistEntriesChildrenOfByIdAndParentCodeAndSortPropertyAndSortOrderAndPageAndPageSizeAsync(
                request.ConceptId,
                detail.Value,
                CodeListEntrySortProperty.Position,
                SortOrder.Ascending,
                1,
                1,
                cancellationToken)
            .GetAwaiter()
            .GetResult()
            .Result
            .Count > 0;

            detail.HasChildren = hasChildren;

            return detail;
        });

        return new PagedResult<CodeListEntryDetail>()
        {
            Page = pageHeader,
            PageSize = pageSizeHeader,
            Results = results,
            TotalCount = totalCount
        };
    }
}
