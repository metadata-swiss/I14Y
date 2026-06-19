namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record MappingTableInputModel
{
    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public required EmailInputModel ResponsiblePerson { get; init; }

    public required UriInputModel Source { get; init; }

    public required UriInputModel Target { get; init; }

    public IEnumerable<CodeInputModel> Themes { get; init; } = [];

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public required string Version { get; init; }
}
