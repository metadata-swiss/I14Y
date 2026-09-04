namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatDatasetInputModel
{
    public required CodeInputModel AccessRights { get; init; }

    public CodeInputModel? ConfidentialityPerson { get; init; }

    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public IReadOnlyCollection<VCardModel> ContactPoints { get; init; } = [];

    public string? DataOwner { get; init; }

    public required MultiLanguageModel Description { get; init; }

    public IReadOnlyCollection<DcatDistributionInputModel> Distributions { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> Documentation { get; init; } = [];

    public CodeInputModel? Frequency { get; init; }

    public IReadOnlyCollection<CodeInputModel> GeoIvIds { get; init; } = [];

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> Images { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> IsReferencedBy { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> LandingPages { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> Languages { get; init; } = [];

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public string? ProcessId { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public IReadOnlyCollection<DcatQualifiedAttributionInputModel> QualifiedAttributions { get; init; } = [];

    public MultiLanguageModel? QualifiedAttributionComplement { get; init; }

    public IReadOnlyCollection<DcatQualifiedRelationInputModel> QualifiedRelations { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> Relations { get; init; } = [];

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public DateTime? RetentionPeriod { get; init; }

    public MultiLanguageModel? RetentionPeriodComplement { get; init; }

    public IReadOnlyCollection<string> Spatial { get; init; } = [];

    public IReadOnlyCollection<PeriodOfTimeModel> TemporalCoverage { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> Themes { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}