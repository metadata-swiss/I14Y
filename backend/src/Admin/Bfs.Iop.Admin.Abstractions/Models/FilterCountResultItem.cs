namespace Bfs.Iop.Admin.Models;

public class FilterCountResultItem
{
    public int Count { get; set; }

    public MultiLanguage? Label { get; set; }

    public string Reference { get; set; } = string.Empty;
}