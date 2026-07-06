namespace Bfs.Iop.Core.Settings;

public sealed record ApiSettings
{
    public required string EnvironmentName { get; init; }
}
