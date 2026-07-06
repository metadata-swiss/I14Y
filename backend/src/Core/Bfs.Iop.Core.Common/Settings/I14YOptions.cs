namespace Bfs.Iop.Core.Settings;

/// <summary>
/// Environment-specific i14y platform options.
/// </summary>
public sealed class I14YOptions
{
    public const string SectionName = "I14Y";

    /// <summary>
    /// IRI base URL used when constructing outbound resource IRIs
    /// (e.g. https://iri.i14y.d.c.bfs.admin.ch in DEV, https://register.ld.admin.ch/i14y in PROD).
    /// NOT used for parsing incoming URIs — parsing is host-agnostic by design (see IriHelpers).
    /// </summary>
    public string IriBaseUrl { get; set; } = "";

    public string MediaBaseUrl { get; set; } = "";
}
