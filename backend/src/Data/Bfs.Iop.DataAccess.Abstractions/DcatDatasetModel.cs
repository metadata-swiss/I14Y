namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatDatasetModel : IPublishableEntityModel
{
    public VocabularyEntryModel AccessRights { get; init; } = null!;

    public VocabularyEntryModel? ConfidentialityPerson { get; init; }

    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public IReadOnlyCollection<VCardModel> ContactPoints { get; init; } = [];

    public string? DataOwner { get; init; }

    public MultiLanguageModel Description { get; init; } = null!;

    public IReadOnlyCollection<DcatDistributionModel> Distributions { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> Documentation { get; init; } = [];

    public VocabularyEntryModel? Frequency { get; init; }

    public IReadOnlyCollection<VocabularyEntryModel> GeoIvIds { get; init; } = [];

    public Guid Id { get; init; }

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> Images { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> IsReferencedBy { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> LandingPages { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> Languages { get; init; } = [];

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public string? ProcessId { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public AgentModel Publisher { get; init; } = null!;

    public IReadOnlyCollection<DcatQualifiedAttributionModel> QualifiedAttributions { get; init; } = [];

    public MultiLanguageModel? QualifiedAttributionComplement { get; init; }

    public IReadOnlyCollection<DcatQualifiedRelationModel> QualifiedRelations { get; init; } = [];

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IReadOnlyCollection<ResourceModel> Relations { get; init; } = [];

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IopPersonModel? ResponsiblePerson { get; init; }

    public DateTime? RetentionPeriod { get; init; }

    public MultiLanguageModel? RetentionPeriodComplement { get; init; }

    public required SystemInfoModel System { get; init; }

    public IReadOnlyCollection<string> Spatial { get; init; } = [];

    public IReadOnlyCollection<PeriodOfTimeModel> TemporalCoverage { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> Themes { get; init; } = [];

    public MultiLanguageModel Title { get; init; } = null!;

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}
