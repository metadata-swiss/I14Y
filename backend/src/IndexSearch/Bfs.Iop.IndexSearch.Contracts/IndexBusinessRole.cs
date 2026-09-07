using System.Text.Json.Serialization;

namespace Bfs.Iop.IndexSearch.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexBusinessRole
{
    Unknown = 0,
    InteroperabilityService = 1,
    LocalDataSteward = 2,
    Submitter = 3,
    StewardshipOrganisationViewer = 4,
    SwissDataSteward = 5,
}