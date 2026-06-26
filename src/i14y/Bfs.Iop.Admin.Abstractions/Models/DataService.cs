using Bfs.Iop.Core.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public sealed class DataService
{
    public VocabularyEntry AccessRights { get; set; } = null!;

    public IEnumerable<Resource> ConformTos { get; set; } = [];

    public IEnumerable<Vcard> ContactPoints { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public IEnumerable<Resource> Documents { get; set; } = [];

    public IEnumerable<Resource> EndpointDescriptions { get; set; } = [];

    public IEnumerable<Resource> EndpointUrls { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; set; } = [];

    public IEnumerable<Resource> LandingPages { get; set; } = [];

    public VocabularyEntry? License { get; set; }

    public DateTimeOffset? Modified { get; init; }

    public IEnumerable<DataServiceVersionSummary> NextVersions { get; set; } = [];

    public DataServiceVersionSummary? PreviousVersion { get; set; }

    public PublicationLevel PublicationLevel { get; set; }

    public PublicationLevel? PublicationLevelProposal { get; set; }

    public MultiLanguage PublisherName { get; set; } = null!;

    public RegistrationStatus RegistrationStatus { get; set; }

    public RegistrationStatus? RegistrationStatusProposal { get; set; }

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IopPersonModel? ResponsiblePerson { get; init; }

    public IEnumerable<IdLabel> ServesDatasets { get; set; } = [];

    public string Status { get; set; } = "UNDEFINED";

    public SystemInfoModel? System { get; set; }

    public IEnumerable<VocabularyEntry> Themes { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;

    public string? Version { get; set; }

    public MultiLanguage? VersionNotes { get; set; }
}