namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record VocabularyEntryModel
{
    public required string Code { get; init; }

    public MultiLanguageModel? Name { get; init; } 

    public string? Uri { get; init; }
}