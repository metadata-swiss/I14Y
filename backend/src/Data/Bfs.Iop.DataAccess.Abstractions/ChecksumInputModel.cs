namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record ChecksumInputModel
{
    public required CodeInputModel Algorithm { get; init; }

    public required string ChecksumValue { get; init; }
}
