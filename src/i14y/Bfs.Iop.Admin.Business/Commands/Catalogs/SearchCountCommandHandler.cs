using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Commands.Catalogs;

internal class SearchCountCommandHandler : IRequestHandler<SearchCountCommand, Models.FilterCountResult>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public SearchCountCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<FilterCountResult> Handle(SearchCountCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetSearchCountByLanguageAndQueryAndAccessRightsAndBusinessEventsAndConceptValueTypesAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAsync(
            null,
            request.Query,
            request.AccessRights,
            request.BusinessEvents,
            request.ConceptValueTypes,
            request.Formats,
            request.PublicationLevels,
            request.PublicationLevelProposals.Where(x => x.HasValue).Select(x => x!.Value),
            request.LifeEvents,
            request.Publishers,
            request.RegistrationStatuses,
            request.RegistrationStatusProposals.Where(x => x.HasValue).Select(x => x!.Value),
            request.Structure,
            request.Themes,
            request.Types,
            cancellationToken
        );

        var catalogSearchCountResult = _mapper.Map<FilterCountResult>(response.Result);

        var unionizedFilterCountResult = new FilterCountResult()
        {
            AccessRights = catalogSearchCountResult.AccessRights,
            BusinessEvents = catalogSearchCountResult.BusinessEvents,
            ConceptValueTypes = catalogSearchCountResult.ConceptValueTypes,
            Formats = catalogSearchCountResult.Formats,
            LifeEvents = catalogSearchCountResult.LifeEvents,
            PublicationLevelProposals = catalogSearchCountResult.PublicationLevelProposals,
            PublicationLevels = catalogSearchCountResult.PublicationLevels,
            Publishers = catalogSearchCountResult.Publishers,
            RegistrationStatuses = catalogSearchCountResult.RegistrationStatuses,
            RegistrationStatusProposals = catalogSearchCountResult.RegistrationStatusProposals,
            Structures = catalogSearchCountResult.Structures,
            Themes = catalogSearchCountResult.Themes,
            Types = catalogSearchCountResult.Types,
            TotalDocCount = catalogSearchCountResult.TotalDocCount
        };

        return unionizedFilterCountResult;
    }
}