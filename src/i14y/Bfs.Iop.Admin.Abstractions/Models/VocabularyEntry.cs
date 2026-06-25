namespace Bfs.Iop.Admin.Models;

public class VocabularyEntry
{
    public string Code { get; set; } = null!;

    public MultiLanguage Name { get; set; } = null!;

    public string? Uri { get; set; }
}