using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using CoreModels = Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.IndexSearch.Contracts.UnitTests;

[TestFixture]
public class SerializationContractTests
{
    // The DataAccess enums carry no JsonConverter attribute of their own, so names on the wire are a
    // property of the host's configuration rather than of the type. Bfs.Iop.IndexSearch.Api adds this
    // same converter in Program.cs; a host that forgets it serialises integers and every caller
    // breaks at once. That is what these two tests pin.
    private static readonly JsonSerializerOptions Configured =
        new() { Converters = { new JsonStringEnumConverter() } };

    [Test]
    public void Enums_serialise_as_names_when_the_host_registers_the_converter()
    {
        JsonSerializer.Serialize(SearchResourceType.MappingTable, Configured).Should().Be("\"MappingTable\"");
        JsonSerializer.Serialize(RegistrationStatus.PreferredStandard, Configured).Should().Be("\"PreferredStandard\"");
        JsonSerializer.Serialize(PublicationLevel.Public, Configured).Should().Be("\"Public\"");
        JsonSerializer.Serialize(ConceptType.CodeList, Configured).Should().Be("\"CodeList\"");
        JsonSerializer.Serialize(CreationType.Automated, Configured).Should().Be("\"Automated\"");
    }


    [Test]
    public void Enums_round_trip_by_name()
    {
        var json = JsonSerializer.Serialize(RegistrationStatus.Superseded, Configured);

        JsonSerializer.Deserialize<RegistrationStatus>(json, Configured)
            .Should().Be(RegistrationStatus.Superseded);
    }

    [Test]
    public void Every_field_of_the_core_search_model_can_be_built_from_a_hit()
    {
        var hitFields = typeof(CatalogSearchHit).GetProperties().Select(x => x.Name).ToHashSet();

        var derived = new Dictionary<string, string>
        {
            ["Publisher"] = "resolved by the caller from PublisherId",
            ["Structure"] = "derived from HasStructure",
            ["System"] = "built from CreatedAt / ModifiedAt / CreationType",
        };

        var missing = typeof(CoreModels.SearchResultModel).GetProperties()
            .Select(x => x.Name)
            .Where(name => !hitFields.Contains(name) && !derived.ContainsKey(name))
            .ToArray();

        missing.Should().BeEmpty();
    }
}
