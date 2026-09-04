using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Relational.Entities;

internal sealed class IopConcept : PublishableEntityBase
{
    public ICollection<CodeListEntry>? CodeListEntries { get; set; }

    public int? CodeListEntryValueMaxLength { get; set; }

    public CodeListEntryValueType? CodeListEntryValueType { get; set; }

    public CodeListEntrySortProperty? CodeListEntryDefaultSortProperty { get; set; }

    public ConceptType ConceptType { get; set; }

    public ICollection<Resource> ConformsTo { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public string[] Identifiers { get; set; } = [];

    public bool IsLocked { get; set; }

    public ICollection<Keyword> Keywords { get; set; } = [];

    public int? MaxLength { get; set; }

    public decimal? MaxValue { get; set; }

    public string? MeasurementUnit { get; set; }

    public int? MinLength { get; set; }

    public decimal? MinValue { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public int? NumberDecimals { get; set; }

    public string? Pattern { get; set; }

    public ICollection<Resource> Replaces { get; set; } = [];

    public IopPerson? ResponsibleDeputy { get; set; }

    public Guid? ResponsibleDeputyId { get; set; }

    public IopPerson ResponsiblePerson { get; set; } = null!;

    public Guid ResponsiblePersonId { get; set; }

    public IList<string> Themes { get; set; } = [];

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = null!;
}
