namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record ChecksumInputModel
{
    public required CodeInputModel Algorithm { get; init; }

    public required string ChecksumValue { get; init; }
}
