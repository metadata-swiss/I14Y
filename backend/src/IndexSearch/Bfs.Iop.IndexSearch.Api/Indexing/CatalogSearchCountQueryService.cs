using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Data.Indexing;
using Bfs.Iop.Core.Vocabularies;
using Bfs.Iop.Search.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Turns the engine's raw facet buckets into the public count model, keyed on
/// <see cref="CatalogFacetDimensions"/>.
/// <para>
/// The dimension identifiers are a UI contract, not an internal detail: admin-ui and public-ui read
/// facets by these names, so renaming one here empties that facet in the UI with no error anywhere.
/// They were carried over verbatim from the previous engine's count handler for exactly that reason.
/// </para>
/// </summary>
internal sealed class CatalogSearchCountQueryService : ICatalogSearchCountQueryService
{
    /// <summary>
    /// Stands in for a facet the engine did not return at all. Its empty dictionary maps to an
    /// empty list, which is what the UI expects for "this filter has no values".
    /// </summary>
    private static readonly CatalogSearchCountResultEntry _defaultWhenNotFound = new()
    {
        Identifier = "default when not found",
    };

    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAgentReader _agents;
    private readonly IVocabularyReader _vocabularies;

    public CatalogSearchCountQueryService(
        ICatalogIndexService catalogIndexService,
        IAgentReader agents,
        IVocabularyReader vocabularies)
    {
        _catalogIndexService = catalogIndexService;
        _agents = agents;
        _vocabularies = vocabularies;
    }

    public async Task<SearchCountResultModel> SearchCountAsync(
        string? query,
        string? language,
        CatalogSearchFilter filter,
        CancellationToken cancellationToken)
    {
        var entries = (await _catalogIndexService.SearchCountAsync(query, language, filter, cancellationToken)).ToList();

        var publishers = await MapAgentsAsync(Facet(entries, CatalogFacetDimensions.PublisherIdentifier).CountByValues, cancellationToken);

        return new SearchCountResultModel
        {
            AccessRights = MapVocabulary(
                Facet(entries, CatalogFacetDimensions.AccessRights).CountByValues,
                _vocabularies.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>()),
            BusinessEvents = MapVocabulary(
                Facet(entries, CatalogFacetDimensions.BusinessEvents).CountByValues,
                _vocabularies.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>()),
            ConceptValueTypes = MapEnum<ConceptType>(Facet(entries, CatalogFacetDimensions.ConceptType).CountByValues),
            Formats = MapVocabulary(
                Facet(entries, CatalogFacetDimensions.Formats).CountByValues,
                _vocabularies.GetExistingOrEmptyVocabulary<FileTypesVocabulary>()),
            LifeEvents = MapVocabulary(
                Facet(entries, CatalogFacetDimensions.LifeEvents).CountByValues,
                _vocabularies.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>()),
            PublicationLevels = MapEnum<PublicationLevel>(Facet(entries, CatalogFacetDimensions.PublicationLevel).CountByValues),
            PublicationLevelProposals = MapEnum<PublicationLevel>(Facet(entries, CatalogFacetDimensions.PublicationLevelProposal).CountByValues),
            Publishers = publishers,
            RegistrationStatuses = MapEnum<RegistrationStatus>(Facet(entries, CatalogFacetDimensions.RegistrationStatus).CountByValues),
            RegistrationStatusProposals = MapEnum<RegistrationStatus>(Facet(entries, CatalogFacetDimensions.RegistrationStatusProposal).CountByValues),
            Structures = MapStrings(Facet(entries, CatalogFacetDimensions.HasStructure).CountByValues)
                .Select(x => new SearchCountResultItem<SearchStructureOption>
                {
                    Count = x.Count,
                    Value = IsTruthy(x.Value)
                        ? SearchStructureOption.WithStructure
                        : SearchStructureOption.WithoutStructure,
                }),
            Themes = MapVocabulary(
                Facet(entries, CatalogFacetDimensions.Themes).CountByValues,
                _vocabularies.GetExistingOrEmptyVocabulary<ThemesVocabulary>()),
            Types = MapStrings(Facet(entries, CatalogFacetDimensions.Type).CountByValues),
            // Every facet carries the same total; Type is used only because it is always present.
            TotalDocCount = Facet(entries, CatalogFacetDimensions.Type).TotalDocumentsCount,
        };
    }

    private static CatalogSearchCountResultEntry Facet(List<CatalogSearchCountResultEntry> entries, string identifier) =>
        entries.SingleOrDefault(x => x.Identifier == identifier, _defaultWhenNotFound);

    /// <summary>
    /// Reads a boolean facet label without throwing.
    /// <para>
    /// Elasticsearch buckets a boolean field on <c>0</c>/<c>1</c> and carries the readable form in
    /// <c>key_as_string</c>. The engine prefers the readable form for this dimension, so the value
    /// here is normally "true"/"false" — but <c>bool.Parse</c> would throw on "0", and a throw here
    /// fails the ENTIRE count response, taking every other facet with it. Accepting both forms keeps
    /// a mapping change from turning into a 500 on every search page.
    /// </para>
    /// </summary>
    private static bool IsTruthy(string value) =>
        value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1";

    private async Task<IEnumerable<SearchCountResultItem<AgentModel>>> MapAgentsAsync(
        IReadOnlyDictionary<string, int> countByValues,
        CancellationToken cancellationToken)
    {
        if (countByValues.Count == 0)
        {
            return [];
        }

        // Keyed by publisher identifier, not by id — see the PublisherIdentifier facet.
        var agents = await _agents.GetAgents(countByValues.Keys, cancellationToken);

        var agentsByIdentifier = agents
            .GroupBy(x => x.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        // An identifier with no matching agent is dropped rather than shown as a nameless facet.
        return countByValues
            .Where(x => agentsByIdentifier.ContainsKey(x.Key))
            .Select(x => new SearchCountResultItem<AgentModel>
            {
                Value = agentsByIdentifier[x.Key],
                Count = x.Value,
            })
            .OrderByDescending(x => x.Count);
    }

    private static IEnumerable<SearchCountResultItem<T>> MapEnum<T>(IReadOnlyDictionary<string, int> countByValues)
        where T : Enum =>
        countByValues
            .Select(x => new SearchCountResultItem<T>
            {
                Value = (T)Enum.Parse(typeof(T), x.Key),
                Count = x.Value,
            })
            .OrderByDescending(x => x.Count);

    private static IEnumerable<SearchCountResultItem<VocabularyEntryModel>> MapVocabulary(
        IReadOnlyDictionary<string, int> countByValues,
        IdentifiedVocabularyBase vocabulary)
    {
        var list = new List<SearchCountResultItem<VocabularyEntryModel>>();

        foreach (var item in countByValues)
        {
            var entry = vocabulary.Entries.SingleOrDefault(x => x.Code == item.Key);

            // Codes absent from the vocabulary are skipped: the index can hold values that came from
            // a data error, and a filter entry with no label is worse than no entry at all.
            if (entry is null)
            {
                continue;
            }

            list.Add(new() { Value = entry, Count = item.Value });
        }

        return list.OrderByDescending(x => x.Count);
    }

    private static IEnumerable<SearchCountResultItem<string>> MapStrings(IReadOnlyDictionary<string, int> countByValues) =>
        countByValues
            .Select(x => new SearchCountResultItem<string> { Value = x.Key, Count = x.Value })
            .OrderByDescending(x => x.Count);
}
