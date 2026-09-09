namespace Bfs.Iop.AuditTrail.ApiClient.Configuration;

public sealed class AuditTrailOptions
{
    public const string SectionName = "AuditTrail";

    public string BaseUrl { get; init; } = "";
}
