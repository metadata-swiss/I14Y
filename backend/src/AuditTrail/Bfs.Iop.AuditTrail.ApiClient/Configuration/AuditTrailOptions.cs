namespace Bfs.Iop.AuditTrail.ApiClient.Configuration;

public sealed class AuditTrailOptions
{
    public const string SectionName = "AuditTrail";

    public required string BaseUrl { get; init; }
}
