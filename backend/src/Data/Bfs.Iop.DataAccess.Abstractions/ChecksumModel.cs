namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record ChecksumModel
{
    public VocabularyEntryModel Algorithm { get; init; } = null!;

    public string ChecksumValue { get; init; } = null!;
}