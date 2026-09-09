using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record SearchCountResultModel
{
    public IEnumerable<SearchCountResultItem<VocabularyEntryModel>> AccessRights { get; init; } = [];

    public IEnumerable<SearchCountResultItem<AgentModel>> AttributedAgents { get; init; } = [];

    public IEnumerable<SearchCountResultItem<VocabularyEntryModel>> BusinessEvents { get; init; } = [];

    public IEnumerable<SearchCountResultItem<ConceptType>> ConceptValueTypes { get; init; } = [];

    public IEnumerable<SearchCountResultItem<VocabularyEntryModel>> Formats { get; init; } = [];

    public IEnumerable<SearchCountResultItem<VocabularyEntryModel>> LifeEvents { get; init; } = [];

    public IEnumerable<SearchCountResultItem<PublicationLevel>> PublicationLevelProposals { get; init; } = [];

    public IEnumerable<SearchCountResultItem<PublicationLevel>> PublicationLevels { get; init; } = [];

    public IEnumerable<SearchCountResultItem<AgentModel>> Publishers { get; init; } = [];

    public IEnumerable<SearchCountResultItem<RegistrationStatus>> RegistrationStatuses { get; init; } = [];

    public IEnumerable<SearchCountResultItem<RegistrationStatus>> RegistrationStatusProposals { get; init; } = [];

    public IEnumerable<SearchCountResultItem<SearchStructureOption>> Structures { get; init; } = [];

    public IEnumerable<SearchCountResultItem<VocabularyEntryModel>> Themes { get; init; } = [];

    public int TotalDocCount { get; init; }

    public IEnumerable<SearchCountResultItem<string>> Types { get; init; } = [];
}
