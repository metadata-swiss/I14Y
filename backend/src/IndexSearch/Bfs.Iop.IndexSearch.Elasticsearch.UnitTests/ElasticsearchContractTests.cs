using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// Everything else in this project asserts that our code produced the JSON we expected. Nothing checks
// that Elasticsearch accepts it. The mapping is ~640 lines of hand-assembled analysers, ngram
// sub-fields and multi-fields; if a server rejects it the host cannot start at all, and no unit test
// would say so.
//
// Off by default: set INDEXSEARCH_TEST_ELASTICSEARCH (defaults to the local compose node when the
// category is run). Uses its own index name so a shared cluster's real indices are never touched.
[TestFixture]
[Explicit("Needs a live Elasticsearch. docker compose up -d elasticsearch")]
[Category("Integration")]
public class ElasticsearchContractTests
{
    private const string CatalogIndex = "i14y-catalog-contracttest";

    private static readonly string[] German = ["de"];

    private HttpClient _client = null!;
    private Guid _datasetId;

    [OneTimeSetUp]
    public async Task CreateIndexAsync()
    {
        var uri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH") ?? "http://localhost:9200";

        // This fixture deletes the index it works on. Keep it to a local node.
        var host = new Uri(uri).Host;

        if (host is not ("localhost" or "127.0.0.1" or "[::1]"))
        {
            Assert.Fail($"Refusing to run against '{host}': this fixture deletes indices. Use a local node.");
        }

        _client = new HttpClient { BaseAddress = new Uri(uri), Timeout = TimeSpan.FromSeconds(30) };

        await _client.DeleteAsync($"/{CatalogIndex}");

        var mapping = CatalogIndexMapping.BuildCreateIndexJson();
        var response = await _client.PutAsync(
            $"/{CatalogIndex}",
            new StringContent(mapping, Encoding.UTF8, "application/json"));

        // The whole point of the fixture: if this body is malformed or names an analyser Elasticsearch
        // does not know, the failure has to be visible here rather than at host startup.
        await ShouldSucceedAsync(response, "create index");
    }

    [OneTimeTearDown]
    public void Dispose() => _client?.Dispose();

    [Test, Order(1)]
    public async Task The_generated_mapping_is_what_the_server_stored()
    {
        var stored = await GetJsonAsync($"/{CatalogIndex}/_mapping");

        var properties = stored.GetProperty(CatalogIndex).GetProperty("mappings").GetProperty("properties");

        // Round-tripped through the server, so this is the mapping Elasticsearch actually applied and
        // not merely the JSON we sent.
        properties.GetProperty("identifier").GetProperty("type").GetString().Should().Be("keyword");
        properties.GetProperty("identifier").GetProperty("fields").GetProperty("text")
            .GetProperty("analyzer").GetString().Should().Be("i14y_text");

        var ngram = properties.GetProperty("title").GetProperty("properties").GetProperty("de")
            .GetProperty("fields").GetProperty("ngram");

        ngram.GetProperty("analyzer").GetString().Should().Be("i14y_ngram");

        // The query text has to be shredded the same way the field was: the index holds nothing but
        // 2-3 character grams, so a search analyser emitting whole words matches none of them. Adding
        // one here would silently return zero partial matches, which is what minimum_should_match on
        // the ngram fields exists to grade.
        ngram.TryGetProperty("search_analyzer", out _).Should().BeFalse();

        properties.GetProperty("registrationStatusWeight").GetProperty("type").GetString().Should().Be("integer");
    }

    [Test, Order(2)]
    public async Task A_document_from_the_factory_is_accepted_and_searchable()
    {
        _datasetId = Guid.NewGuid();

        var (id, document) = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = _datasetId,
            Type = SearchResourceType.Dataset,
            Identifiers = ["BFS-STAT-2024", "BFS-STAT-OLD"],
            PublisherId = Guid.NewGuid(),
            PublisherIdentifier = "CH_BFS",
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Standard,
            CreationType = CreationType.Manual,
            Title = new MultiLanguageModel { De = "Bevölkerungsstatistik", Fr = "Statistique de la population" },
            Description = new MultiLanguageModel { De = "Eine Beschreibung" },
            Keywords = [new MultiLanguageModel { De = "Bevölkerung" }],
            Version = "1.2.0",
            DataOwner = "Bundesamt für Statistik",
            AccessRights = "PUBLIC",
            Themes = ["ENER"],
            Formats = ["CSV"],
            HasStructure = true,
            ResponsiblePerson = new IndexPerson { GivenName = "Ada", FamilyName = "Lovelace", Email = "ada@example.ch" },
            ContactPoints =
            [
                new IndexContactPoint { HasEmail = "kontakt@example.ch", HasTelephone = "+41 58 000 00 00" },
            ],
        });

        var indexed = await _client.PutAsync(
            $"/{CatalogIndex}/_doc/{id}?refresh=true",
            JsonContent.Create(document));

        await ShouldSucceedAsync(indexed, "index document");

        // Every claim the unit tests make about searchability, now decided by the search engine.
        (await SearchHitsAsync("Bevölkerungsstatistik")).Should().Be(1, "the title should match");
        (await SearchHitsAsync("BFS-STAT-2024")).Should().Be(1, "the identifier should match");
        (await SearchHitsAsync("BFS-STAT-OLD")).Should().Be(1, "a superseded identifier should match too");
        (await SearchHitsAsync("statistik")).Should().Be(1, "the data owner should match, case-insensitively");
        (await SearchHitsAsync(_datasetId.ToString())).Should().Be(1, "a pasted id should match");
        (await SearchHitsAsync("kontakt@example.ch")).Should().Be(1, "an address on its own should match");
        (await SearchHitsAsync("+41 58 000 00 00")).Should().Be(1, "a telephone number should match");
        (await SearchHitsAsync("Lovelace")).Should().Be(1, "the responsible person's name should match");
        (await SearchHitsAsync("1.2.0")).Should().Be(1, "the version should match");

        (await SearchHitsAsync("Wetterdaten")).Should().Be(0, "an unrelated term should not match");
    }

    [Test, Order(3)]
    public async Task The_count_body_and_its_aggregations_are_accepted()
    {
        var body = CatalogQueryBuilder.BuildCountBody(null, German, null, SearchCaller.Anonymous);

        var response = await _client.PostAsync(
            $"/{CatalogIndex}/_search",
            JsonContent.Create(body));

        // 13 filter aggregations plus a total, built by hand. A rejected agg would only surface as an
        // empty facet panel in both front-ends.
        await ShouldSucceedAsync(response, "count query");

        var aggregations = (await ReadJsonAsync(response)).GetProperty("aggregations");

        aggregations.GetProperty("Total").GetProperty("doc_count").GetInt32().Should().Be(1);
        aggregations.GetProperty("Themes").GetProperty("values").GetProperty("buckets")[0]
            .GetProperty("key").GetString().Should().Be("ENER");

        // Every dimension has to come back, not just the one asserted above: a rejected or misnamed
        // aggregation shows up as a silently empty facet panel.
        foreach (var (dimension, _) in CatalogQueryBuilder.Dimensions)
        {
            aggregations.TryGetProperty(dimension, out var bucket).Should()
                .BeTrue($"the {dimension} facet should be aggregated");

            bucket.TryGetProperty("values", out _).Should()
                .BeTrue($"the {dimension} facet should carry its buckets");
        }
    }

    [Test, Order(4)]
    public async Task An_anonymous_caller_cannot_see_an_internal_resource()
    {
        var (id, document) = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = SearchResourceType.Dataset,
            Title = new MultiLanguageModel { De = "Geheime Bevölkerungsstatistik" },
            PublicationLevel = PublicationLevel.Internal,
            RegistrationStatus = RegistrationStatus.Standard,
            CreationType = CreationType.Manual,
        });

        await ShouldSucceedAsync(
            await _client.PutAsync($"/{CatalogIndex}/_doc/{id}?refresh=true", JsonContent.Create(document)),
            "index internal document");

        // The authorization clause decided by the server, not by a JSON comparison.
        (await SearchHitsAsync("Bevölkerungsstatistik")).Should().Be(1, "only the public one is visible");

        var steward = new SearchCaller { Role = BusinessRole.InteroperabilityService };

        (await SearchHitsAsync("Bevölkerungsstatistik", steward))
            .Should().Be(2, "an interoperability service sees both");
    }

    private async Task<int> SearchHitsAsync(string query, SearchCaller? caller = null)
    {
        var body = CatalogQueryBuilder.BuildSearchBody(
            query, German, null, caller ?? SearchCaller.Anonymous, from: 0, size: 20);

        var response = await _client.PostAsync($"/{CatalogIndex}/_search", JsonContent.Create(body));

        await ShouldSucceedAsync(response, $"search '{query}'");

        return (await ReadJsonAsync(response)).GetProperty("hits").GetProperty("total")
            .GetProperty("value").GetInt32();
    }

    private async Task<JsonElement> GetJsonAsync(string path)
    {
        var response = await _client.GetAsync(path);
        await ShouldSucceedAsync(response, $"GET {path}");
        return await ReadJsonAsync(response);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();

    // Elasticsearch explains itself well; surface its own words rather than just a status code.
    private static async Task ShouldSucceedAsync(HttpResponseMessage response, string what)
    {
        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"{what} failed with {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
