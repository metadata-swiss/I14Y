namespace Bfs.Iop.Infrastructure.Security.Configuration;

internal sealed class EiamConfiguration : IAuthorizationConfiguration
{
    public string Audience { get; set; } = "";

    public string[] Issuers { get; set; } = [];
}
