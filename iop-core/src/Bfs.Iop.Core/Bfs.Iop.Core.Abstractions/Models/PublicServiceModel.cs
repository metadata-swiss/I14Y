namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record PublicServiceModel : IPublishableEntityModel
{
    public IEnumerable<VocabularyEntryModel> BusinessEvents { get; init; } = [];

    public IEnumerable<ChannelModel> Channels { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public Guid Id { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<IdModel> IsDescribedAt { get; init; } = [];

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> Languages { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> LifeEvents { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public required AgentModel Publisher { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IEnumerable<IdModel> Relations { get; init; } = [];

    public IEnumerable<IdModel> Requires { get; init; } = [];

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IopPersonModel? ResponsiblePerson { get; init; }

    public IEnumerable<VocabularyEntryModel> Sectors { get; init; } = [];

    public required SystemInfoModel System { get; init; }

    public IEnumerable<string> Spatial { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> ThematicAreas { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> SpatialCH {  get; init; } = [];
}
