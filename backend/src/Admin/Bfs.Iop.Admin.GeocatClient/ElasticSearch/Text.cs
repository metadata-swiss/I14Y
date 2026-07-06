using Newtonsoft.Json;

namespace Bfs.Iop.Admin.GeocatClient.ElasticSearch;

internal class Text
{
    [JsonProperty("langger")]
    public string De { get; set; }

    public string Default { get; set; }

    [JsonProperty("langeng")]
    public string En { get; set; }

    [JsonProperty("langfre")]
    public string Fr { get; set; }

    [JsonProperty("langita")]
    public string It { get; set; }
}