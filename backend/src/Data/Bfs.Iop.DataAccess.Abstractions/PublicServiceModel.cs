namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record PublicServiceModel : IPublishableEntityModel
{
    public IReadOnlyCollection<VocabularyEntryModel> BusinessEvents { get; init; } = [];

    public IReadOnlyCollection<ChannelModel> Channels { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public Guid Id { get; init; }

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public IReadOnlyCollection<IdModel> IsDescribedAt { get; init; } = [];

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> Languages { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> LifeEvents { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public required AgentModel Publisher { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IReadOnlyCollection<IdModel> Relations { get; init; } = [];

    public IReadOnlyCollection<IdModel> Requires { get; init; } = [];

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IopPersonModel? ResponsiblePerson { get; init; }

    public IReadOnlyCollection<VocabularyEntryModel> Sectors { get; init; } = [];

    public required SystemInfoModel System { get; init; }

    public IReadOnlyCollection<string> Spatial { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> ThematicAreas { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> SpatialCH {  get; init; } = [];
}
