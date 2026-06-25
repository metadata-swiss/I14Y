using Newtonsoft.Json;

namespace Bfs.Iop.Admin.OpenDataClient.PackageSearch;

internal class SearchResultItem
{
    public Text Description { get; set; }

    public string Id { get; set; }

    [JsonProperty("name")]
    public string LinkId { get; set; }

    public Text Title { get; set; }
}