namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record VocabularyModel
{
    public IReadOnlyList<VocabularyEntryModel> Entries { get; init; } = [];

    public required string Identifier { get; init; }
}