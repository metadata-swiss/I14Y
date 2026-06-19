namespace Bfs.Iop.Infrastructure.Security.Configuration;

internal sealed class SecurityConfiguration : ISecurityConfiguration
{
    private readonly List<IAuthorizationConfiguration> _configurations;

    public SecurityConfiguration() => 
        _configurations = [];

    public IReadOnlyList<IAuthorizationConfiguration> Configurations => 
        _configurations.AsReadOnly();

    public void AddConfiguration(IAuthorizationConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var set = _configurations.SelectMany(x => x.Issuers).Intersect(configuration.Issuers);

        if (set.Any())
        {
            throw new ArgumentException($"A configuration with the issuer(s) '{string.Join(", ", set)}' already exists.");
        }

        _configurations.Add(configuration);
    }
}
