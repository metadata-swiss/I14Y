using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Lucene;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

internal sealed class GetCatalogSearchCountCommandHandler : IRequestHandler<GetCatalogSearchCountCommand, SearchCountResultModel>
{
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAgentsService _agentsService;
    private readonly IVocabulariesService _vocabulariesService;

    private static readonly CatalogSearchCountResultEntry _defaultWhenNotFound = new() 
    { 
        Identifier = "default when not found" 
    };

    public GetCatalogSearchCountCommandHandler(
        ICatalogIndexService catalogIndexService,
        IAgentsService agentsService,
        IVocabulariesService vocabulariesService)
    {
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
    }

    public async Task<SearchCountResultModel> Handle(GetCatalogSearchCountCommand request, CancellationToken cancellationToken)
    {
        var indexResults = _catalogIndexService.SearchCount(request.QueryString, request.Language, request.Filter).ToList();

        var publishers = await MapAgentModelsFromDictionaryAsync(
            indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.PublisherIdentifier, _defaultWhenNotFound).CountByValues,
            cancellationToken);

        var attributedAgents = await MapAgentModelsFromDictionaryAsync(
            indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.QualifiedAttributionAgentIdentifier, _defaultWhenNotFound).CountByValues,
            cancellationToken);

        var results = new SearchCountResultModel()
        {
            AccessRights = MapVocabularyFromDictionary(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.AccessRights, _defaultWhenNotFound).CountByValues,
                _vocabulariesService.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>()),
            AttributedAgents = attributedAgents,
            BusinessEvents = MapVocabularyFromDictionary(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.BusinessEvents, _defaultWhenNotFound).CountByValues,
                _vocabulariesService.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>()),
            ConceptValueTypes = MapEnumFromDictionary<ConceptType>(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.ConceptType, _defaultWhenNotFound).CountByValues),
            Formats = MapVocabularyFromDictionary(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.Formats, _defaultWhenNotFound).CountByValues,
                _vocabulariesService.GetExistingOrEmptyVocabulary<FileTypesVocabulary>()),
            LifeEvents = MapVocabularyFromDictionary(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.LifeEvents, _defaultWhenNotFound).CountByValues,
                _vocabulariesService.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>()),
            PublicationLevels = MapEnumFromDictionary<PublicationLevel>(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.PublicationLevel, _defaultWhenNotFound).CountByValues),
            PublicationLevelProposals = MapEnumFromDictionary<PublicationLevel>(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.PublicationLevelProposal, _defaultWhenNotFound).CountByValues),
            Publishers = publishers,
            RegistrationStatuses = MapEnumFromDictionary<RegistrationStatus>(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.RegistrationStatus, _defaultWhenNotFound).CountByValues),
            RegistrationStatusProposals = MapEnumFromDictionary<RegistrationStatus>(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.RegistrationStatusProposal, _defaultWhenNotFound).CountByValues),
            Structures = MapStringsFromDictionary(indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.HasStructure, _defaultWhenNotFound).CountByValues)
                .Select(x => new SearchCountResultItem<SearchStructureOption>()
                {
                    Count = x.Count, 
                    Value = bool.Parse(x.Value) is true 
                        ? SearchStructureOption.WithStructure 
                        : SearchStructureOption.WithoutStructure 
                }),
            Themes = MapVocabularyFromDictionary(
                indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.Themes, _defaultWhenNotFound).CountByValues,
                _vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>()),
            Types = MapStringsFromDictionary(indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.Type, _defaultWhenNotFound).CountByValues),
            TotalDocCount = indexResults.SingleOrDefault(x => x.Identifier == LuceneFields.Catalog.Type, _defaultWhenNotFound).TotalDocumentsCount
        };

        return results;
    }

    private async Task<IEnumerable<SearchCountResultItem<AgentModel>>> MapAgentModelsFromDictionaryAsync(
        IReadOnlyDictionary<string, int> countByValues,
        CancellationToken cancellationToken)
    {
        if (countByValues.Count == 0)
        {
            return [];
        }
        
        // countByValues is keyed by publisher identifier (see the PublisherIdentifier facet).
        var agents = await _agentsService.GetAgents(countByValues.Keys, cancellationToken);

        var agentsByIdentifier = agents
            .GroupBy(x => x.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        return countByValues
            .Where(x => agentsByIdentifier.ContainsKey(x.Key))
            .Select(x => new SearchCountResultItem<AgentModel>()
            {
                Value = agentsByIdentifier[x.Key],
                Count = x.Value
            })
            .OrderByDescending(x => x.Count);
    }

    private static IEnumerable<SearchCountResultItem<T>> MapEnumFromDictionary<T>(IReadOnlyDictionary<string, int> countByValues) where T : Enum
    {
        var results = countByValues.Keys.Select(x =>
        {
            return new SearchCountResultItem<T>
            {
                Value = (T)Enum.Parse(typeof(T), x),
                Count = countByValues[x]
            };
        });

        return results.OrderByDescending(x => x.Count);
    }

    private static IEnumerable<SearchCountResultItem<VocabularyEntryModel>> MapVocabularyFromDictionary(
        IReadOnlyDictionary<string, int> countByValues,
        IdentifiedVocabularyBase vocabulary)
    {
        var list = new List<SearchCountResultItem<VocabularyEntryModel>>();

        foreach (var item in countByValues)
        {
            var entry = vocabulary.Entries.SingleOrDefault(x => x.Code == item.Key);

            if (entry is null)
            {
                continue;
            }

            list.Add(new()
            {
                Value = entry,
                Count = item.Value
            });
        }

        return list.OrderByDescending(x => x.Count);
    }

    private static IEnumerable<SearchCountResultItem<string>> MapStringsFromDictionary(
        IReadOnlyDictionary<string, int> countByValues)
    {
        var results = countByValues.Keys.Select(x =>
        {
            return new SearchCountResultItem<string>()
            {
                Value = x,
                Count = countByValues[x]
            };
        });

        return results.OrderByDescending(x => x.Count);
    }
}
