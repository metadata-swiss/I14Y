namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record MappingTableModel : IPublishableEntityModel
{
    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public Guid Id { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

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

    public IEnumerable<VocabularyEntryModel> Themes { get; init; } = [];

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }
    
    public required string Version { get; init; }
}
