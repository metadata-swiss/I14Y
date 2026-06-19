namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record ResourceModel
{
    public MultiLanguageModel? Label { get; init; }

    public required string Uri { get; init; }
}
