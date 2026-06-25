using AutoMapper;
using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Extensions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView.CodeListEntries;

internal sealed class GetCodeListEntriesForAutoCompleteCommandHandler : 
    IRequestHandler<GetCodeListEntriesForAutoCompleteCommand, PagedResult<CodelistEntryInput>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetCodeListEntriesForAutoCompleteCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<PagedResult<CodelistEntryInput>> Handle(GetCodeListEntriesForAutoCompleteCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesAutoCompleteByIdAndCodePrefixAndSortPropertyAndSortOrderAndPageAndPageSizeAsync(
            request.ConceptId,
            request.CodePrefix,
            request.SortProperty,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var codeListEntries = response.Result.ToList();

        int pageHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeHeader = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        return new PagedResult<CodelistEntryInput>()
        {
            Page = pageHeader,
            PageSize = pageSizeHeader,
            Results = _mapper.Map<IEnumerable<CodelistEntryInput>>(codeListEntries),
            TotalCount = totalCount
        };
    }
}
