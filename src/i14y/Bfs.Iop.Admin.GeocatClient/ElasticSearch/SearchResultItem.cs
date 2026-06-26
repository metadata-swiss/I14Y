using Newtonsoft.Json;

namespace Bfs.Iop.Admin.GeocatClient.ElasticSearch;

internal class SearchResultItem
{
    [JsonProperty("_source")]
    public Metadata Metadata { get; set; }
}