using System.Text.Json.Serialization;

namespace Bfs.Iop.IndexSearch.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexStructureOption
{
    WithStructure = 1,
    WithoutStructure = 2,
}