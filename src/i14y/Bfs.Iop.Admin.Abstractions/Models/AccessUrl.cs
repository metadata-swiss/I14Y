namespace Bfs.Iop.Admin.Models;

public class AccessUrl
{
    public string Href { get; set; } = null!;

    public bool IsDownload { get; set; } = false;

    public MultiLanguage Label { get; set; } = null!;
}