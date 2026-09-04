namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DataServiceModel : IPublishableEntityModel
{
    public VocabularyEntryModel AccessRights { get; init; } = null!;

    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public IReadOnlyCollection<VCardModel> ContactPoints { get; init; } = [];

    public MultiLanguageModel Description { get; init; } = null!;

    public IReadOnlyCollection<ResourceModel> Documentation { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> EndpointDescriptions { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> EndpointUrls { get; init; } = [];

    public Guid Id { get; init; }

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> LandingPages { get; init; } = [];

    public VocabularyEntryModel? License { get; init; }

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public AgentModel Publisher { get; init; } = null!;

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IopPersonModel? ResponsiblePerson { get; init; }

    public IReadOnlyCollection<IdModel> ServesDatasets { get; init; } = [];

    public required SystemInfoModel System { get; init; }

    public IReadOnlyCollection<VocabularyEntryModel> Themes { get; init; } = [];

    public MultiLanguageModel Title { get; init; } = null!;

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}
