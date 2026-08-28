using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using CoreModels = Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.IndexSearch.Contracts.UnitTests;

[TestFixture]
public class SerializationContractTests
{
    private static readonly JsonSerializerOptions Default = new();

    [Test]
    public void Enums_serialise_as_names_without_any_host_configuration()
    {
        JsonSerializer.Serialize(IndexResourceType.MappingTable, Default).Should().Be("\"MappingTable\"");
        JsonSerializer.Serialize(IndexRegistrationStatus.PreferredStandard, Default).Should().Be("\"PreferredStandard\"");
        JsonSerializer.Serialize(IndexPublicationLevel.Public, Default).Should().Be("\"Public\"");
        JsonSerializer.Serialize(IndexConceptType.CodeList, Default).Should().Be("\"CodeList\"");
        JsonSerializer.Serialize(IndexCreationType.Automated, Default).Should().Be("\"Automated\"");
    }

    [Test]
    public void Enums_round_trip_by_name()
    {
        var json = JsonSerializer.Serialize(IndexRegistrationStatus.Superseded, Default);

        JsonSerializer.Deserialize<IndexRegistrationStatus>(json, Default)
            .Should().Be(IndexRegistrationStatus.Superseded);
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
