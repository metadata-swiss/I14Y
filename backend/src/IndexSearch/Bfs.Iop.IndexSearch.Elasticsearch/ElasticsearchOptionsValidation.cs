using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchOptionsValidation : IValidateOptions<ElasticsearchOptions>
{
    // appsettings.json ships "#{ELASTICSEARCH_URI}#" for the pipeline to replace. Nothing in the repo
    // performs that replacement, so an unsubstituted token is a real deployment state and it is not
    // empty — the check has to look for the token itself, not just for a missing value.
    private const string TokenMarker = "#{";

    public ValidateOptionsResult Validate(string? name, ElasticsearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        Require(failures, "Uri", options.Uri);
        Require(failures, "CatalogIndexName", options.CatalogIndexName);
        Require(failures, "CodeListIndexName", options.CodeListIndexName);

        if (failures.Count == 0)
        {
            if (!Uri.TryCreate(options.Uri, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                failures.Add($"'{Key("Uri")}' is not an absolute http or https URL: '{options.Uri}'.");
            }

            if (string.Equals(options.CatalogIndexName, options.CodeListIndexName, StringComparison.Ordinal))
            {
                // They would resolve to one alias, so a rebuild would have each pass overwrite the
                // other's documents and every search would answer from the wrong corpus.
                failures.Add(
                    $"'{Key("CatalogIndexName")}' and '{Key("CodeListIndexName")}' are both "
                    + $"'{options.CatalogIndexName}'; they have to name different indices.");
            }
        }

        ValidateCredentials(failures, options);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    /// <summary>
    ///     Credentials are optional - the cluster may run without security - so they cannot go through
    ///     <see cref="Require"/>. What they must not be is half configured or still a placeholder.
    /// </summary>
    private static void ValidateCredentials(List<string> failures, ElasticsearchOptions options)
    {
        RejectPlaceholder(failures, "Username", options.Username);
        RejectPlaceholder(failures, "Password", options.Password);

        var hasUsername = !string.IsNullOrWhiteSpace(options.Username);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);

        if (hasUsername != hasPassword)
        {
            failures.Add(
                $"'{Key("Username")}' and '{Key("Password")}' have to be set together or left blank "
                + $"together; only '{Key(hasUsername ? "Username" : "Password")}' is configured.");
        }
    }

    private static void RejectPlaceholder(List<string> failures, string key, string? value)
    {
        if (value?.Contains(TokenMarker, StringComparison.Ordinal) == true)
        {
            failures.Add(
                $"'{Key(key)}' still holds the unsubstituted placeholder '{value}'. Set it to an empty "
                + "value when the cluster runs without authentication; leaving it out is not the same "
                + "thing, because the placeholder is what appsettings.json falls back to.");
        }
    }

    private static void Require(List<string> failures, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"'{Key(key)}' is not configured.");
            return;
        }

        if (value.Contains(TokenMarker, StringComparison.Ordinal))
        {
            failures.Add($"'{Key(key)}' still holds the unsubstituted placeholder '{value}'.");
        }
    }

    private static string Key(string key) => $"{ElasticsearchOptions.SectionName}:{key}";
}
