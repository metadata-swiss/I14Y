using Bfs.Iop.Core.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public sealed class CatalogEntry
{
    public VocabularyEntry? AccessRights { get; set; }

    public VocabularyEntry[] BusinessEvents { get; set; } = [];

    public ConceptType? ConceptValueType { get; set; }

    public MultiLanguage Description { get; set; } = null!;

    public VocabularyEntry[]? Formats { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = [];

    public VocabularyEntry[] LifeEvents { get; set; } = [];

    public PublicationLevel PublicationLevel { get; set; }

    public PublicationLevel? PublicationLevelProposal { get; set; }

    public MultiLanguage PublisherName { get; set; } = null!;

    public RegistrationStatus RegistrationStatus { get; set; }

    public RegistrationStatus? RegistrationStatusProposal { get; set; }

    public SearchStructureOption? Structure { get; set; }

    public SystemInfoModel? System { get; set; }

    public VocabularyEntry[] Themes { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string? Version { get; set; }
}