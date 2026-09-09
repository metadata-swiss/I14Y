using Newtonsoft.Json;

namespace Bfs.Iop.Admin.GeocatClient.ElasticSearch;

internal class SearchResultSummary
{
    [JsonProperty("value")]
    public int TotalCount { get; set; }
}