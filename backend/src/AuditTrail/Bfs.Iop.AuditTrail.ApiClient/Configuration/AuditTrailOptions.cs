namespace Bfs.Iop.AuditTrail.ApiClient.Configuration;

public sealed class AuditTrailOptions
{
    public const string SectionName = "AuditTrail";

    public required string BaseUrl { get; init; }

    public int EnsureResourceIsTrackedMaxAttempts { get; init; } = 20;

    public int EnsureResourceIsTrackedDelayInMs { get; init; } = 300;
}
