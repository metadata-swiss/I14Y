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

internal class GetAllPagedCommandHandler
    : IRequestHandler<GetAllPagedCommand, PagedResult<CodeListEntryDetail>>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public GetAllPagedCommandHandler(IIopCoreApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<PagedResult<CodeListEntryDetail>> Handle(GetAllPagedCommand request, CancellationToken cancellationToken)
    {
        var codeListEntriesByConceptIdResponse = await _apiClient
            .GetConceptsCodelistEntriesByIdAndSortPropertyAndSortOrderAndPageAndPageSizeAsync(
                request.ConceptId,
                sortProperty: null, // defined by the concept metadata
                SortOrder.Ascending,
                request.Page,
                request.PageSize,
                cancellationToken);

        var codeListEntries = codeListEntriesByConceptIdResponse.Result.ToList();

        int page = codeListEntriesByConceptIdResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSize = codeListEntriesByConceptIdResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = codeListEntriesByConceptIdResponse.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        return new PagedResult<CodeListEntryDetail>()
        {
            Page = page,
            PageSize = pageSize,
            Results = _mapper.Map<IEnumerable<CodeListEntryDetail>>(codeListEntries),
            TotalCount = totalCount
        };
    }
}