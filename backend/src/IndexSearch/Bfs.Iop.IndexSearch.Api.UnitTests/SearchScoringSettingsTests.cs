using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

[TestFixture]
internal sealed class SearchScoringSettingsTests
{
    [Test]
    public void Appsettings_ships_exactly_the_tuned_defaults()
    {
        var shipped = new SearchScoringOptions();

        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .GetSection(SearchScoringOptions.SectionName)
            .Bind(shipped);

        JsonSerializer.Serialize(shipped).Should().Be(JsonSerializer.Serialize(new SearchScoringOptions()));
    }

    [Test]
    public void Appsettings_lists_every_weighted_field()
    {
        var listed = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .GetSection($"{SearchScoringOptions.SectionName}:Catalog:FieldWeights")
            .GetChildren()
            .Select(x => x.Key);

        listed.Should().BeEquivalentTo(new SearchScoringOptions().Catalog.FieldWeights.Keys);
    }
}
