using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView.CodeListEntries;

internal sealed class GetPagedChildrenCodeListEntriesCommandHandler : 
    IRequestHandler<GetPagedChildrenCodeListEntriesCommand, PagedResult<CodeListEntryDetail>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetPagedChildrenCodeListEntriesCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<PagedResult<CodeListEntryDetail>> Handle(GetPagedChildrenCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        var getChildCodeListEntriesByCodeResponse = await _apiClient.GetConceptsCodelistEntriesChildrenOfByIdAndParentCodeAndSortPropertyAndSortOrderAndPageAndPageSizeAsync(
            request.ConceptId,
            request.Code,
            request.SortProperty,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken
            );

        var codeListEntries = getChildCodeListEntriesByCodeResponse.Result.ToList();

        int pageHeader = getChildCodeListEntriesByCodeResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = getChildCodeListEntriesByCodeResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = getChildCodeListEntriesByCodeResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

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
            Results = results.ToList(),
            TotalCount = totalCount
        };
    }
}
