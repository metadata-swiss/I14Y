namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record AgentInputModel
{
    public CodeInputModel? Classification { get; init; }

    public VCardModel? ContactPoint { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public string? HomePage { get; init; }

    public required string Identifier { get; init; }

    public IReadOnlyCollection<ResourceModel> Images { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required MultiLanguageModel PrefLabel { get; init; }

    public IReadOnlyCollection<string> Spatial { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> SpatialCH { get; init; } = [];

    public IReadOnlyCollection<IdModel> SubAgents { get; init; } = [];

    public string? Uid { get; init; }
}
