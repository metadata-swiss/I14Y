namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record SearchResultModel
{
    public VocabularyEntryModel? AccessRights { get; init; }

    public IEnumerable<VocabularyEntryModel> BusinessEvents { get; init; } = [];

    public ConceptType? ConceptType { get; init; }

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<VocabularyEntryModel> Formats { get; init; } = [];

    public Guid Id { get; init; }

    public required string Identifier { get; init; }

    public IEnumerable<VocabularyEntryModel> LifeEvents { get; init; } = [];

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public required AgentModel Publisher { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public SearchStructureOption? Structure { get; init; }

    public SystemInfoModel? System { get; init; }

    public IEnumerable<VocabularyEntryModel> Themes { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }

    public required SearchResourceType Type { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public string? Version { get; init; }
}
