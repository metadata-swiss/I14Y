namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatDatasetModel : IPublishableEntityModel
{
    public VocabularyEntryModel AccessRights { get; init; } = null!;

    public VocabularyEntryModel? ConfidentialityPerson { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public IEnumerable<VCardModel> ContactPoints { get; init; } = [];

    public string? DataOwner { get; init; }

    public MultiLanguageModel Description { get; init; } = null!;

    public IEnumerable<DcatDistributionModel> Distributions { get; init; } = [];

    public IEnumerable<ResourceModel> Documentation { get; init; } = [];

    public VocabularyEntryModel? Frequency { get; init; }

    public IEnumerable<VocabularyEntryModel> GeoIvIds { get; init; } = [];

    public Guid Id { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public IEnumerable<ResourceModel> IsReferencedBy { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public IEnumerable<ResourceModel> LandingPages { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> Languages { get; init; } = [];

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public string? ProcessId { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public AgentModel Publisher { get; init; } = null!;

    public IEnumerable<DcatQualifiedAttributionModel> QualifiedAttributions { get; init; } = [];

    public MultiLanguageModel? QualifiedAttributionComplement { get; init; }

    public IEnumerable<DcatQualifiedRelationModel> QualifiedRelations { get; init; } = [];

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IEnumerable<ResourceModel> Relations { get; init; } = [];

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IopPersonModel? ResponsiblePerson { get; init; }

    public DateTime? RetentionPeriod { get; init; }

    public MultiLanguageModel? RetentionPeriodComplement { get; init; }

    public required SystemInfoModel System { get; init; }

    public IEnumerable<string> Spatial { get; init; } = [];

    public IEnumerable<PeriodOfTimeModel> TemporalCoverage { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> Themes { get; init; } = [];

    public MultiLanguageModel Title { get; init; } = null!;

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}
