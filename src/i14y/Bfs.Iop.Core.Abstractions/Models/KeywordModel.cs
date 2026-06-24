namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record KeywordModel
{
    public MultiLanguageModel? Label { get; init; }

    public string? Uri { get; init; }
}
