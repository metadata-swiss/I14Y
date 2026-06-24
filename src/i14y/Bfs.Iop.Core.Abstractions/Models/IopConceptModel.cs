namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IopConceptModel : IPublishableEntityModel
{
    public IEnumerable<CodeListEntryModel>? CodeListEntries { get; set; }

    public CodeListEntrySortProperty? CodeListEntryDefaultSortProperty { get; init; }

    public int? CodeListEntryValueMaxLength { get; init; }

    public CodeListEntryValueType? CodeListEntryValueType { get; init; }

    public ConceptType ConceptType { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public Guid Id { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public bool IsLocked { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public int? MaxLength { get; init; }

    public decimal? MaxValue { get; init; }

    public string? MeasurementUnit { get; init; }

    public int? MinLength { get; init; }

    public decimal? MinValue { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public int? NumberDecimals { get; init; }

    public string? Pattern { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public required AgentModel Publisher { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    // Security constrain, it can only be accessed by someone with token.
    public IopPersonModel? ResponsibleDeputy { get; init; }

    // Security constrain, it can only be accessed by someone with token.
    public required IopPersonModel? ResponsiblePerson { get; init; }

    public required SystemInfoModel System { get; init; }

    public IEnumerable<VocabularyEntryModel> Themes { get; init; } = [];

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public required string Version { get; set; }
}
