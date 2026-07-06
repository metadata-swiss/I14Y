namespace Bfs.Iop.Admin.Models;

public class DcatVocabularyEntry
{
    public string Code { get; set; } = null!;

    public MultiLanguage? Name { get; set; }

    public string ThemeTaxonomy { get; set; } = string.Empty;

    public string? Uri { get; set; }
}