namespace Bfs.Iop.IndexSearch.ApiClient.Extensions;

/// <summary>
/// The configuration keys for reaching the IndexSearch service, and the single definition of what
/// counts as "configured".
/// <para>
/// Exists because that predicate was previously written three times — in Core's forwarding
/// registration, in Core's health-check guard, and in Admin's registration — and had already
/// diverged: two rejected an unsubstituted <c>#{TOKEN}#</c> placeholder and one did not. The
/// consequence was that a deployment which forgot to substitute the token registered a health check
/// for a client that was never registered, so <c>/health</c> threw on resolution — the endpoint whose
/// job is reporting health being the thing that broke.
/// </para>
/// </summary>
public static class IndexSearchConfiguration
{
    public const string SectionName = "IndexSearchApiClient";

    public const string BaseUrlKey = SectionName + ":BaseUrl";

    public const string SecretKey = SectionName + ":Secret";

    /// <summary>
    /// Whether a usable base URL was supplied.
    /// <para>
    /// Rejects an unreplaced <c>#{TOKEN}#</c> placeholder as well as an empty value: appsettings.json
    /// ships those tokens, and an unsubstituted one would otherwise look configured and send every
    /// call to an unusable address.
    /// </para>
    /// </summary>
    public static bool IsConfigured(string? baseUrl) =>
        !string.IsNullOrWhiteSpace(baseUrl)
        && !(baseUrl.StartsWith("#{", StringComparison.Ordinal)
            && baseUrl.EndsWith("}#", StringComparison.Ordinal));
}
