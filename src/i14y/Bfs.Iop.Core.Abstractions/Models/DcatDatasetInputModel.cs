namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatDatasetInputModel
{
    public required CodeInputModel AccessRights { get; init; }

    public CodeInputModel? ConfidentialityPerson { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public IEnumerable<VCardModel> ContactPoints { get; init; } = [];

    public string? DataOwner { get; init; }

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<DcatDistributionInputModel> Distributions { get; init; } = [];

    public IEnumerable<ResourceModel> Documentation { get; init; } = [];

    public CodeInputModel? Frequency { get; init; }

    public IEnumerable<CodeInputModel> GeoIvIds { get; init; } = [];

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public IEnumerable<ResourceModel> IsReferencedBy { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public IEnumerable<ResourceModel> LandingPages { get; init; } = [];

    public IEnumerable<CodeInputModel> Languages { get; init; } = [];

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public string? ProcessId { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public IEnumerable<DcatQualifiedAttributionInputModel> QualifiedAttributions { get; init; } = [];

    public MultiLanguageModel? QualifiedAttributionComplement { get; init; }

    public IEnumerable<DcatQualifiedRelationInputModel> QualifiedRelations { get; init; } = [];

    public IEnumerable<ResourceModel> Relations { get; init; } = [];

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public DateTime? RetentionPeriod { get; init; }

    public MultiLanguageModel? RetentionPeriodComplement { get; init; }

    public IEnumerable<string> Spatial { get; init; } = [];

    public IEnumerable<PeriodOfTimeModel> TemporalCoverage { get; init; } = [];

    public IEnumerable<CodeInputModel> Themes { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}