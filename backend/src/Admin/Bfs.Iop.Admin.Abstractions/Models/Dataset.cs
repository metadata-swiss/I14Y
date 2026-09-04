using Bfs.Iop.DataAccess.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class Dataset
{
    public VocabularyEntry AccessRights { get; set; } = null!;

    public VocabularyEntry? ConfidentialityPerson { get; set; }

    public IEnumerable<Resource> ConformTos { get; set; } = null!;

    public IEnumerable<Vcard> ContactPoints { get; set; } = null!;

    public string? DataOwner { get; set; }

    public MultiLanguage Description { get; set; } = null!;

    public IEnumerable<DcatDistributionModel> Distributions { get; init; } = [];

    public IEnumerable<Resource> Documents { get; set; } = null!;

    public VocabularyEntry? Frequency { get; set; } = null;

    public IEnumerable<VocabularyEntry> GeoIvId { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = new List<string>();

    public IEnumerable<Resource>? Image { get; set; }

    public IEnumerable<Resource> IsReferencedBy { get; set; } = new List<Resource>();

    public IEnumerable<KeywordModel> Keywords { get; set; } = null!;

    public IEnumerable<Resource> LandingPages { get; set; } = null!;

    public IEnumerable<string> Languages { get; set; } = null!;

    public DateTimeOffset? LastUpdated { get; set; } = null;

    public IEnumerable<DatasetVersionSummary> NextVersions { get; set; } = null!;

    public DatasetVersionSummary? PreviousVersion { get; set; } = null;

    public string? ProcessId { get; set; }

    public PublicationLevel PublicationLevel { get; set; }

    public PublicationLevel? PublicationLevelProposal { get; set; }

    public DateTimeOffset? Published { get; set; } = null;

    public MultiLanguage PublisherName { get; set; } = null!;

    public IEnumerable<QualifiedAttribution>? QualifiedAttribution { get; set; }

    public MultiLanguage? QualifiedAttributionComplement { get; set; }

    public IEnumerable<QualifiedRelation>? QualifiedRelation { get; set; }

    public RegistrationStatus RegistrationStatus { get; set; }

    public RegistrationStatus? RegistrationStatusProposal { get; set; }

    public IEnumerable<Resource>? Relations { get; set; } = null!;

    public Person? ResponsiblePerson { get; set; }

    public Person? ResponsiblePersonDeputy { get; set; }

    public DateTime? RetentionPeriod { get; set; }

    public MultiLanguage? RetentionPeriodDescription { get; set; }

    public SystemInfoModel? System { get; set; }

    public IEnumerable<string> SpatialCoverages { get; set; } = null!;

    public string Status { get; set; } = "UNDEFINED";

    public IEnumerable<PeriodOfTime> TemporalCoverage { get; set; } = null!;

    public IEnumerable<VocabularyEntry> Themes { get; set; } = null!;

    public MultiLanguage Title { get; set; } = null!;

    public string? Version { get; set; } = null!;

    public MultiLanguage VersionNotes { get; set; } = null!;
}