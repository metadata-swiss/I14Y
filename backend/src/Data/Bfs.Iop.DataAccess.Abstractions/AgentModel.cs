namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record AgentModel
{
    public VocabularyEntryModel? Classification { get; init; }

    public VCardModel? ContactPoint { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public string? HomePage { get; init; }

    public Guid Id { get; init; }

    public required string Identifier { get; init; }

    public IReadOnlyCollection<ResourceModel> Images { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required MultiLanguageModel PrefLabel { get; init; }

    public required SystemInfoModel System { get; init; }

    public IReadOnlyCollection<string> Spatial { get; init; } = [];

    public IReadOnlyCollection<VocabularyEntryModel> SpatialCH { get; init; } = [];

    public IReadOnlyCollection<IdNameModel> SubAgents { get; init ; } = [];

    public string? Uid { get; init; }
}
