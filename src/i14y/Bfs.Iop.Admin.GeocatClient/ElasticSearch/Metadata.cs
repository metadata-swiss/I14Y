using Newtonsoft.Json;

namespace Bfs.Iop.Admin.GeocatClient.ElasticSearch
{
    internal class Metadata
    {
        [JsonProperty("resourceAbstractObject")]
        public Text Abstract { get; set; }

        [JsonProperty("uuid")]
        public string Id { get; set; }

        [JsonProperty("resourceTitleObject")]
        public Text Title { get; set; }
    }
}