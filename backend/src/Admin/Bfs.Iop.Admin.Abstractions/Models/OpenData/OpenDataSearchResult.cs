using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models.OpenData;

public sealed class OpenDataSearchResult
{
    public int Count { get; set; }

    public int From { get; set; }

    public ICollection<OpenDataSearchResultItem> Items { get; set; } = [];

    public int To { get; set; }
}