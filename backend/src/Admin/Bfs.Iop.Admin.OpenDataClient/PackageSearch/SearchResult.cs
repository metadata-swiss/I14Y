using System.Collections.Generic;

namespace Bfs.Iop.Admin.OpenDataClient.PackageSearch;

internal class SearchResult
{
    public int Count { get; set; }

    public List<SearchResultItem> Results { get; set; }
}