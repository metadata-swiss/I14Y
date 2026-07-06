using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.GeocatClient.ElasticSearch;

internal class SearchResult
{
    [JsonProperty("max_score")]
    public float? MaxScore { get; set; }

    [JsonProperty("hits")]
    public List<SearchResultItem> Metadata { get; set; }

    [JsonProperty("total")]
    public SearchResultSummary Summary { get; set; }
}