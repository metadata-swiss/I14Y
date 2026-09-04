namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record MappingTableInputModel
{
    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public required EmailInputModel ResponsiblePerson { get; init; }

    public required UriInputModel Source { get; init; }

    public required UriInputModel Target { get; init; }

    public IReadOnlyCollection<CodeInputModel> Themes { get; init; } = [];

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public required string Version { get; init; }
}
