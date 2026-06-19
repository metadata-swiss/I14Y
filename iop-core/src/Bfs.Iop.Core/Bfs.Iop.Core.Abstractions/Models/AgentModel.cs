namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record AgentModel
{
    public VocabularyEntryModel? Classification { get; init; }

    public VCardModel? ContactPoint { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public string? HomePage { get; init; }

    public Guid Id { get; init; }

    public required string Identifier { get; init; }

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required MultiLanguageModel PrefLabel { get; init; }

    public required SystemInfoModel System { get; init; }

    public IEnumerable<string> Spatial { get; init; } = [];

    public IEnumerable<VocabularyEntryModel> SpatialCH { get; init; } = [];

    public IEnumerable<IdNameModel> SubAgents { get; init ; } = [];

    public string? Uid { get; init; }
}
