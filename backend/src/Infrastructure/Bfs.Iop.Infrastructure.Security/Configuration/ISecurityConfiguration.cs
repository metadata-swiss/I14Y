namespace Bfs.Iop.Infrastructure.Security.Configuration;

internal interface ISecurityConfiguration
{
    IReadOnlyList<IAuthorizationConfiguration> Configurations { get; }

    void AddConfiguration(IAuthorizationConfiguration configuration);
}
