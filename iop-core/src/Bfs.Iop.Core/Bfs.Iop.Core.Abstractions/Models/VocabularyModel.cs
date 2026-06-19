namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record VocabularyModel
{
    public IReadOnlyList<VocabularyEntryModel> Entries { get; init; } = [];

    public required string Identifier { get; init; }
}