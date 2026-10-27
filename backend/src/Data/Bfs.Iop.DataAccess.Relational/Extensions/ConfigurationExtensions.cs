using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace Bfs.Iop.DataAccess.Relational.Extensions;

public static class ConfigurationExtensions
{
    public static string GetPostgresDatabaseConnectionString(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        const string postgresCredentialsSectionKey = "postgresCredentialsSectionKey";
        const string databaseNameKey = "database";
        const string hostnameKey = "hostname";
        const string usernameKey = "username";
        const string passwordKey = "password";
        const string portKey = "port";
        const string poolingOptionsKey = "poolingOptions";

        var section = configuration.GetValue<string>(postgresCredentialsSectionKey) ??
            throw new ConfigurationErrorsException("Postgres credentials section key is not configured.");

        var database = configuration[$"{section}:{databaseNameKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{databaseNameKey}'", paramName: nameof(configuration));

        var host = configuration[$"{section}:{hostnameKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{hostnameKey}'", paramName: nameof(configuration));

        var password = configuration[$"{section}:{passwordKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{passwordKey}'", paramName: nameof(configuration));

        var port = configuration[$"{section}:{portKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{portKey}'", paramName: nameof(configuration));

        var username = configuration[$"{section}:{usernameKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{usernameKey}'", paramName: nameof(configuration));

        var baseConnectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";

        var poolingOptions = configuration[$"{section}:{poolingOptionsKey}"];

        return string.IsNullOrWhiteSpace(poolingOptions)
            ? baseConnectionString
            : $"{baseConnectionString};{poolingOptions}";

    }
}
