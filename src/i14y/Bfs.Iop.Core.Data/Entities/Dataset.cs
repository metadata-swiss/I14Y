namespace Bfs.Iop.Core.Data.Entities;

internal class Dataset : VersionableEntity<Dataset>
{
    public string AccessRights { get; set; } = string.Empty;

    public string? ConfidentialityPerson { get; set; }

    public List<Resource> ConformsTo { get; set; } = [];

    public List<VCard> ContactPoint { get; set; } = [];

    public string? DataOwner { get; set; }

    public List<DataServiceDataset> DataServices { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public List<Distribution> Distributions { get; set; } = [];

    public List<Resource> Documentation { get; set; } = [];

    public string? Frequency { get; set; }

    public string[] GeoIvId { get; set; } = [];

    public string[] Identifier { get; set; } = [];

    public List<Resource> Image { get; set; } = [];

    public List<Resource> IsReferencedBy { get; set; } = [];

    public DateTimeOffset? Issued { get; set; } = null!;

    public List<Keyword> Keyword { get; set; } = [];

    public List<Resource> LandingPage { get; set; } = [];

    public string[] Language { get; set; } = [];

    public DateTimeOffset? Modified { get; set; }

    public string[]? OtherIdentifier { get; set; }

    public string? ProcessId { get; set; }

    public List<QualifiedAttribution> QualifiedAttribution { get; set; } = [];

    public MultiLanguage? QualifiedAttributionComplement { get; set; }

    public List<QualifiedRelation> QualifiedRelation { get; set; } = [];

    public List<Resource> Relation { get; set; } = [];

    public IopPerson? ResponsiblePerson { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public IopPerson? ResponsibleDeputy { get; set; }

    public Guid? ResponsibleDeputyId { get; set; }

    public DateTime? RetentionPeriod { get; set; }

    public MultiLanguage? RetentionPeriodDescription { get; set; }

    public string[]? Spatial { get; set; }

    public List<PeriodOfTime> TemporalCoverage { get; set; } = [];

    public string[] Theme { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;
}