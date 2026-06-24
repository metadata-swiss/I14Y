namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DataServiceModel : IPublishableEntityModel
{
    public VocabularyEntryModel AccessRights { get; init; } = null!;

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public IEnumerable<VCardModel> ContactPoints { get; init; } = [];

    public MultiLanguageModel Description { get; init; } = null!;

    public IEnumerable<ResourceModel> Documentation { get; init; } = [];

    public IEnumerable<ResourceModel> EndpointDescriptions { get; init; } = [];

    public IEnumerable<ResourceModel> EndpointUrls { get; init; } = [];

    public Guid Id { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public IEnumerable<ResourceModel> LandingPages { get; init; } = [];

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

    public IEnumerable<IdModel> ServesDatasets { get; init; } = [];

    public required SystemInfoModel System { get; init; }

    public IEnumerable<VocabularyEntryModel> Themes { get; init; } = [];

    public MultiLanguageModel Title { get; init; } = null!;

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}
