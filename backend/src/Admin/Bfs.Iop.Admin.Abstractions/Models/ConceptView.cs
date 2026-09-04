using Bfs.Iop.DataAccess.Abstractions;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed class ConceptView
{
    public int? CodelistEntryValueMaxLength { get; set; }

    public CodeListEntryValueTypeEnum? CodeListEntryValueType { get; set; }

    public CodeListEntrySortProperty? CodeListEntryDefaultSortProperty { get; set; }

    public Guid? CodeListId { get; set; }

    public ConceptType ConceptType { get; set; }

    public IEnumerable<Resource>? ConformsTo { get; set; }

    public MultiLanguage Description { get; set; } = new();

    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = [];

    public bool IsLocked { get; set; }

    public IEnumerable<KeywordModel>? Keywords { get; set; }

    public int? MaxLength { get; set; }

    public decimal? MaxValue { get; set; }

    public string? MeasurementUnit { get; set; }

    public int? MinLength { get; set; }

    public decimal? MinValue { get; set; }

    public MultiLanguage Name { get; set; } = new();

    public int? NbDecimal { get; set; }

    public string? Pattern { get; set; }

    public IEnumerable<ConceptReferenceModel> Replaces { get; set; } = [];

    public IEnumerable<ConceptReferenceModel> IsReplacedBy { get; set; } = [];

    public required AgentModel Publisher { get; set; }

    public Person? ResponsibleDeputy { get; set; }

    public Person ResponsiblePerson { get; set; } = new();

    public SystemInfoModel? System { get; set; }

    public IEnumerable<VocabularyEntry>? Themes { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = string.Empty;
}