using Bfs.Iop.DataAccess.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class ConceptInput
{
    public int? CodelistEntryValueMaxLength { get; set; }

    public CodeListEntryValueTypeEnum? CodeListEntryValueType { get; set; }

    public CodeListEntrySortProperty? CodeListEntryDefaultSortProperty { get; set; }

    public ConceptType ConceptType { get; set; }

    public IEnumerable<Resource>? ConformsTo { get; set; }

    public MultiLanguage Description { get; set; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = [];

    public IEnumerable<KeywordModel>? Keywords { get; set; }

    public int? MaxLength { get; set; }

    public decimal? MaxValue { get; set; }

    public string? MeasurementUnit { get; set; }

    public int? MinLength { get; set; }

    public decimal? MinValue { get; set; }

    public MultiLanguage Name { get; set; } = default!;

    public int? NbDecimal { get; set; }

    public string? Pattern { get; set; }

    public IEnumerable<IdModel> Replaces { get; set; } = [];

    public required IdentifierInputModel Publisher { get; set; }

    public Person? ResponsibleDeputy { get; set; }

    public Person ResponsiblePerson { get; set; } = null!;

    public IEnumerable<string>? ThemeCodes { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = default!;
}