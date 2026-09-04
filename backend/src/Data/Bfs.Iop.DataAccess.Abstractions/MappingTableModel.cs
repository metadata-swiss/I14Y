namespace Bfs.Iop.DataAccess.Abstractions;

public record MappingTableModel : IPublishableEntityModel
{
    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public Guid Id { get; init; }

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public required AgentModel Publisher { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public required IopPersonModel? ResponsiblePerson { get; init; }

    public required SystemInfoModel System { get; init; }

    public required MappingTableUriModel Source { get; init; }

    public required MappingTableUriModel Target { get; init; }

    public IReadOnlyCollection<VocabularyEntryModel> Themes { get; init; } = [];

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }
    
    public required string Version { get; init; }
}
