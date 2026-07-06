using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.Commands.Catalogs;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Extensions;
using MapsterMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.Catalogs;

internal sealed class SearchCommandHandler : IRequestHandler<SearchCommand, PagedResult<CatalogEntry>>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public SearchCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<PagedResult<CatalogEntry>> Handle(SearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetSearchByLanguageAndQueryAndAccessRightsAndBusinessEventsAndConceptValueTypesAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSizeAsync(
            null,
            request.Query,
            request.AccessRights,
            request.BusinessEvents,
            request.ConceptValueTypes,
            request.Formats,
            request.PublicationLevels,
            request.PublicationLevelProposals?.Where(x => x.HasValue).Select(x => x!.Value),
            request.LifeEvents,
            request.Publishers,
            request.RegistrationStatuses,
            request.RegistrationStatusProposals?.Where(x => x.HasValue).Select(x => x!.Value),
            request.Structure,
            request.Themes,
            request.Types,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        int pageValue = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageHeaderKey);
        int pageSizeValue = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.PageSizeHeaderKey);
        int totalCount = response.TryGetSwaggerHeaderIntValue(HttpContextExtensions.TotalRowsHeaderKey);

        var catalogEntries = _mapper.Map<IEnumerable<CatalogEntry>>(response.Result);

        return new PagedResult<CatalogEntry>
        {
            Page = pageValue,
            PageSize = pageSizeValue,
            Results = catalogEntries,
            TotalCount = totalCount
        };
    }
}