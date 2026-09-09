using System.Text.RegularExpressions;

namespace Bfs.Iop.DataAccess.Relational.Tools;

public static class IriHelper
{
    // Matches /concept/{identifier}/version/{version}
    private static readonly Regex _conceptIriRegex =
        new(@"/concept/([^/]+)/version/([^/]+)", RegexOptions.Compiled);

    // Matches /concept/{identifier}/{code}/version/{version}
    private static readonly Regex _conceptCodeIriRegex =
        new(@"/concept/([^/]+)/([^/]+)/version/([^/]+)", RegexOptions.Compiled);

    // Matches /version/{version} anywhere in the path
    private static readonly Regex _versionRegex =
        new(@"/version/([^/]+)", RegexOptions.Compiled);

    /// <summary>
    /// Extracts concept identifier and version from any i14y concept IRI,
    /// regardless of host (works for DEV, ABN, PROD URLs).
    /// </summary>
    public static bool TryExtractConceptIdentifierAndVersion(
        string uri, out string identifier, out string version)
    {
        var match = _conceptIriRegex.Match(uri);
        identifier = match.Success ? match.Groups[1].Value : string.Empty;
        version = match.Success ? match.Groups[2].Value : string.Empty;
        return match.Success;
    }

    /// <summary>
    /// Extracts the code segment from a concept code-list entry IRI
    /// (<c>/concept/{identifier}/{code}/version/{version}</c>), regardless of host.
    /// </summary>
    public static bool TryExtractCodeFromConceptCodeIri(string uri, out string code)
    {
        var match = _conceptCodeIriRegex.Match(uri);
        code = match.Success ? match.Groups[2].Value : string.Empty;
        return match.Success;
    }

    /// <summary>
    /// Extracts the version segment from any IRI containing <c>/version/{version}</c>.
    /// Returns <c>null</c> when no version segment is present.
    /// </summary>
    public static string? ExtractVersion(string uri)
    {
        var match = _versionRegex.Match(uri);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Builds a concept IRI from a base URL, identifier and version.
    /// </summary>
    public static string BuildConceptIri(string baseIriUrl, string identifier, string version) =>
        $"{baseIriUrl.TrimEnd('/')}/concept/{identifier}/version/{version}";
}
