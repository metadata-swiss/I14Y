namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record ResourceModel
{
    public MultiLanguageModel? Label { get; init; }

    public required string Uri { get; init; }
}
