namespace Bfs.Iop.Admin.Models;

public sealed class LocalizerConfiguration
{
    public const string SectionName = "Localizer";

    public string[] OrderedFallbackLanguageCodes { get; set; } = [];
}