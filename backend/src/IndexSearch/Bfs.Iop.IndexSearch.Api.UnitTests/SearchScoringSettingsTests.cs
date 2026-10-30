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
    public void Appsettings_lists_every_setting()
    {
        var shippedFile = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .GetSection(SearchScoringOptions.SectionName);

        using var defaultsJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(new SearchScoringOptions()));

        var defaults = new ConfigurationBuilder()
            .AddJsonStream(defaultsJson)
            .Build();

        SettingPaths(shippedFile).Should().BeEquivalentTo(SettingPaths(defaults));
    }

    private static IEnumerable<string> SettingPaths(IConfiguration configuration) =>
        configuration
            .AsEnumerable(makePathsRelative: true)
            .Where(x => x.Value is not null)
            .Select(x => x.Key.ToLowerInvariant());
}
