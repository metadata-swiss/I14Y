namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record PublicServiceInputModel
{
    public IReadOnlyCollection<CodeInputModel> BusinessEvents { get; init; } = [];

    public IReadOnlyCollection<ChannelInputModel> Channels { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public IReadOnlyCollection<IdModel> IsDescribedAt { get; init; } = [];

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> Languages { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> LifeEvents { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public IReadOnlyCollection<IdModel> Relations { get; init; } = [];

    public IReadOnlyCollection<IdModel> Requires { get; init; } = [];

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public IReadOnlyCollection<CodeInputModel> Sectors { get; init; } = [];

    public IReadOnlyCollection<string> Spatial { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> ThematicAreas { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> SpatialCH { get; init; } = [];
}
