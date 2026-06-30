namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IopConceptInputModel
{
    public CodeListEntrySortProperty? CodeListEntryDefaultSortProperty { get; init; }

    public CodeListEntryValueType? CodeListEntryValueType { get; init; }

    public int? CodeListEntryValueMaxLength { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public required ConceptType ConceptType { get; init; }

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public int? MaxLength { get; init; }

    public decimal? MaxValue { get; init; }

    public string? MeasurementUnit { get; init; }

    public int? MinLength { get; init; }

    public decimal? MinValue { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public int? NumberDecimals { get; init; }

    public string? Pattern { get; init; }

    public IEnumerable<IdModel> Replaces { get; init; } = [];

    public required IdentifierInputModel Publisher { get; init; }

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public required EmailInputModel ResponsiblePerson { get; init; }

    public IEnumerable<CodeInputModel> Themes { get; init; } = [];

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public required string Version { get; init; }
}
