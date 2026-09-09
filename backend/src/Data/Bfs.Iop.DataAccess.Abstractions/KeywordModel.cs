namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record KeywordModel
{
    public MultiLanguageModel? Label { get; init; }

    public string? Uri { get; init; }
}
