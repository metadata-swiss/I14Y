namespace Bfs.Iop.Infrastructure.Security.Configuration;

internal sealed class KeycloakConfiguration : IAuthorizationConfiguration
{
    public string Audience { get; set; } = "";

    public string[] Issuers { get; set; } = [];
}