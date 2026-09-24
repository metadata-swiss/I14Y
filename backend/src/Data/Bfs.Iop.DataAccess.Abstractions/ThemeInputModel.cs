namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record ThemeInputModel
{
    public string? Code { get; init; }

    public string? Uri { get; init; }
}
