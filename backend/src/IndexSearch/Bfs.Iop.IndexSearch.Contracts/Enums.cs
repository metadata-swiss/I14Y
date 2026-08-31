using System.Text.Json.Serialization;

namespace Bfs.Iop.IndexSearch.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexResourceType
{
    Dataset = 1,
    DataService = 2,
    PublicService = 3,
    Concept = 4,
    MappingTable = 5,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexPublicationLevel
{
    Internal = 1,
    Public = 2,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexRegistrationStatus
{
    Incomplete = 1,
    Candidate = 2,
    Recorded = 3,
    Qualified = 4,
    Standard = 5,
    PreferredStandard = 6,
    Superseded = 7,
    Retired = 8,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexConceptType
{
    CodeList = 1,
    Date = 2,
    Numeric = 3,
    String = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexCreationType
{
    Manual = 0,
    Automated = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexStructureOption
{
    WithStructure = 1,
    WithoutStructure = 2,
}

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
