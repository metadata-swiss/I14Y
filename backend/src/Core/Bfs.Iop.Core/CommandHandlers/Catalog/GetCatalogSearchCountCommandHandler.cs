using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;
using MediatR;

using CatalogFacetRequest = Bfs.Iop.IndexSearch.Contracts.Search.CatalogFacetRequest;
using IIndexSearchApiClient = Bfs.Iop.IndexSearch.ApiClient.IIndexSearchApiClient;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

internal sealed class GetCatalogSearchCountCommandHandler
    : IRequestHandler<GetCatalogSearchCountCommand, SearchCountResultModel>
{
    private readonly IIndexSearchApiClient _search;
    private readonly IAgentsService _agentsService;
    private readonly IVocabulariesService _vocabulariesService;

    public GetCatalogSearchCountCommandHandler(
        IIndexSearchApiClient search,
        IAgentsService agentsService,
        IVocabulariesService vocabulariesService)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

        _vocabulariesService = vocabulariesService ??
            throw new ArgumentNullException(nameof(vocabulariesService));
    }

    public async Task<SearchCountResultModel> Handle(
        GetCatalogSearchCountCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _search.PostSearchCatalogFacetsByBodyAsync(
            new CatalogFacetRequest
            {
                Query = request.QueryString,
                Language = request.Language,
                Filter = request.Filter.MapToIndexSearchFilter(),
            },
            cancellationToken);

        var facets = response.Result;

        var publishers = await Agents(facets.Publishers, cancellationToken);
        var attributedAgents = await Agents(facets.AttributedAgents, cancellationToken);

        return new SearchCountResultModel
        {
            AccessRights = Vocabulary<RightsStatementsVocabulary>(facets.AccessRights),
            AttributedAgents = attributedAgents,
            BusinessEvents = Vocabulary<BkBusinessEventsVocabulary>(facets.BusinessEvents),
            ConceptValueTypes = Enums<ConceptType>(facets.ConceptTypes),
            Formats = Vocabulary<FileTypesVocabulary>(facets.Formats),
            LifeEvents = Vocabulary<BkLifeEventsVocabulary>(facets.LifeEvents),
            PublicationLevels = Enums<PublicationLevel>(facets.PublicationLevels),
            PublicationLevelProposals = Enums<PublicationLevel>(facets.PublicationLevelProposals),
            Publishers = publishers,
            RegistrationStatuses = Enums<RegistrationStatus>(facets.RegistrationStatuses),
            RegistrationStatusProposals = Enums<RegistrationStatus>(facets.RegistrationStatusProposals),
            Structures = Structures(facets.Structures),
            Themes = Vocabulary<ThemesVocabulary>(facets.Themes),
            TotalDocCount = facets.TotalCount,
            Types = Ordered(Counts(facets.Types).Select(x => Item(x.Key, x.Value))),
        };
    }

    private async Task<IEnumerable<SearchCountResultItem<AgentModel>>> Agents(
        IReadOnlyDictionary<string, int>? countByIdentifier,
        CancellationToken cancellationToken)
    {
        var counts = Counts(countByIdentifier);

        if (counts.Count == 0)
        {
            return [];
        }

        var agents = await _agentsService.GetAgents(counts.Keys, cancellationToken);

        var byIdentifier = agents
            .GroupBy(x => x.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);


        return Ordered(counts
            .Where(x => byIdentifier.ContainsKey(x.Key))
            .Select(x => Item(byIdentifier[x.Key], x.Value)));
    }


    private IEnumerable<SearchCountResultItem<VocabularyEntryModel>> Vocabulary<T>(
        IReadOnlyDictionary<string, int>? countByCode)
        where T : IdentifiedVocabularyBase, new()
    {
        var counts = Counts(countByCode);

        if (counts.Count == 0)
        {
            return [];
        }

        var entries = _vocabulariesService.GetExistingOrEmptyVocabulary<T>().Entries
            .GroupBy(x => x.Code)
            .ToDictionary(x => x.Key, x => x.First());

        // A code the vocabulary does not carry is dropped: there is nothing to label the count with.
        return Ordered(counts
            .Where(x => entries.ContainsKey(x.Key))
            .Select(x => Item(entries[x.Key], x.Value)));
    }

    private static IEnumerable<SearchCountResultItem<T>> Enums<T>(IReadOnlyDictionary<string, int>? countByName)
        where T : struct, Enum =>
        Ordered(Counts(countByName)
            .Select(x => (Parsed: Enum.TryParse<T>(x.Key, out var value) ? value : (T?)null, x.Value))
            .Where(x => x.Parsed.HasValue)
            .Select(x => Item(x.Parsed!.Value, x.Value)));

    private static IEnumerable<SearchCountResultItem<SearchStructureOption>> Structures(
        IReadOnlyDictionary<string, int>? countByFlag) =>
        Ordered(Counts(countByFlag)
            .Select(x => (Parsed: bool.TryParse(x.Key, out var flag) ? flag : (bool?)null, x.Value))
            .Where(x => x.Parsed.HasValue)
            .Select(x => Item(
                x.Parsed!.Value ? SearchStructureOption.WithStructure : SearchStructureOption.WithoutStructure,
                x.Value)));

    private static IReadOnlyDictionary<string, int> Counts(IReadOnlyDictionary<string, int>? counts) =>
        counts ?? new Dictionary<string, int>();

    private static SearchCountResultItem<T> Item<T>(T value, int count) => new() { Value = value, Count = count };

    private static IEnumerable<SearchCountResultItem<T>> Ordered<T>(IEnumerable<SearchCountResultItem<T>> items) =>
        items.OrderByDescending(x => x.Count).ToList();
}
