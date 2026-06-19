namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record PublicServiceInputModel
{
    public IEnumerable<CodeInputModel> BusinessEvents { get; init; } = [];

    public IEnumerable<ChannelInputModel> Channels { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public IEnumerable<IdModel> IsDescribedAt { get; init; } = [];

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public IEnumerable<CodeInputModel> Languages { get; init; } = [];

    public IEnumerable<CodeInputModel> LifeEvents { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public IEnumerable<IdModel> Relations { get; init; } = [];

    public IEnumerable<IdModel> Requires { get; init; } = [];

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public IEnumerable<CodeInputModel> Sectors { get; init; } = [];

    public IEnumerable<string> Spatial { get; init; } = [];

    public IEnumerable<CodeInputModel> ThematicAreas { get; init; } = [];

    public IEnumerable<CodeInputModel> SpatialCH { get; init; } = [];
}
