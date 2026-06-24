using System.Drawing;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record AgentInputModel
{
    public CodeInputModel? Classification { get; init; }

    public VCardModel? ContactPoint { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public string? HomePage { get; init; }

    public required string Identifier { get; init; }

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public required MultiLanguageModel Name { get; init; }

    public required MultiLanguageModel PrefLabel { get; init; }

    public IEnumerable<string> Spatial { get; init; } = [];

    public IEnumerable<CodeInputModel> SpatialCH { get; init; } = [];

    public IEnumerable<IdModel> SubAgents { get; init; } = [];

    public string? Uid { get; init; }
}
