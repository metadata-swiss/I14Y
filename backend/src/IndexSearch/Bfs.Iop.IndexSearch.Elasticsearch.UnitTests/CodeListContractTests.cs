using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeList;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// The code list is 99% of the indexed volume and the whole cost of a rebuild, so its mapping earns the
// same treatment as the catalog's: decided by a real server, not by comparing our JSON to itself.
//
// Off by default: set INDEXSEARCH_TEST_ELASTICSEARCH. Uses its own index name.
[TestFixture]
[Explicit("Needs a live Elasticsearch. docker compose up -d elasticsearch")]
[Category("Integration")]
public class CodeListContractTests
{
    private const string Index = "i14y-codelist-contracttest";

    private static readonly Guid ConceptId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task CreateIndexAsync()
    {
        var uri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH") ?? "http://localhost:9200";
        var host = new Uri(uri).Host;

        if (host is not ("localhost" or "127.0.0.1" or "[::1]"))
        {
            Assert.Fail($"Refusing to run against '{host}': this fixture deletes indices. Use a local node.");
        }

        _client = new HttpClient { BaseAddress = new Uri(uri), Timeout = TimeSpan.FromSeconds(30) };

        await _client.DeleteAsync($"/{Index}");

        var response = await _client.PutAsync(
            $"/{Index}",
            new StringContent(CodeListIndexMapping.BuildCreateIndexJson(), Encoding.UTF8, "application/json"));

        await ShouldSucceedAsync(response, "create index");

        // One entry whose two annotations each hold half of what a filter will ask for. This is the
        // shape that tells nested and flattened apart.
        await IndexAsync(new CodeListIndexDocument
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ConceptId = ConceptId,
            Code = "01.02",
            ParentCode = "01",
            Name = new MultiLanguageModel { De = "Bevölkerung", Fr = "Population" },
            Description = new MultiLanguageModel { De = "Eine Beschreibung" },
            Annotations =
            [
                new AnnotationInputModel { Type = "note", Title = "Erste", Uri = "https://example.org/a" },
                new AnnotationInputModel { Type = "warning", Title = "Zweite", Text = new MultiLanguageModel { De = "Achtung" } },
            ],
        });
    }

    [OneTimeTearDown]
    public void Dispose() => _client?.Dispose();

    [Test]
    public async Task The_generated_mapping_is_what_the_server_stored()
    {
        var properties = (await GetJsonAsync($"/{Index}/_mapping"))
            .GetProperty(Index).GetProperty("mappings").GetProperty("properties");

        properties.GetProperty("annotations").GetProperty("type").GetString().Should().Be("nested");

        // Code is filtered and sorted exactly, and searched as text at the highest boost of all.
        properties.GetProperty("code").GetProperty("type").GetString().Should().Be("keyword");
        properties.GetProperty("code").GetProperty("fields").GetProperty("text")
            .GetProperty("type").GetString().Should().Be("text");

        // Name carries the ngram copy; description deliberately does not.
        properties.GetProperty("name").GetProperty("properties").GetProperty("de")
            .GetProperty("fields").TryGetProperty("ngram", out _).Should().BeTrue();

        properties.GetProperty("description").GetProperty("properties").GetProperty("de")
            .TryGetProperty("fields", out _).Should().BeFalse("ngram on 490k descriptions is not free");

        // The annotation text keeps an exact copy for the configured filters.
        properties.GetProperty("annotations").GetProperty("properties").GetProperty("text")
            .GetProperty("properties").GetProperty("de").GetProperty("fields")
            .GetProperty("raw").GetProperty("type").GetString().Should().Be("keyword");
    }

    [Test]
    public async Task An_annotation_filter_matches_within_one_annotation_and_not_across_two()
    {
        // Both halves present, but on the SAME annotation.
        (await NestedAnnotationHitsAsync(("type", "note"), ("title", "erste")))
            .Should().Be(1, "type and title from the same annotation should match");

        // Both halves present on the entry, but split across two annotations. Lucene evaluated each
        // filter inside a single annotation document before joining, so this must NOT match. A
        // flattened object mapping would return the entry here — that is the bug nested prevents.
        (await NestedAnnotationHitsAsync(("type", "note"), ("title", "zweite")))
            .Should().Be(0, "type from one annotation must not pair with a title from another");
    }

    [Test]
    public async Task A_filter_ignores_case_while_the_stored_value_keeps_it()
    {
        var stored = (await GetJsonAsync($"/{Index}/_doc/11111111-1111-1111-1111-111111111111"))
            .GetProperty("_source").GetProperty("annotations")[0];

        // A hit renders from _source, so lowercasing on the way in would show the front-end "erste".
        stored.GetProperty("title").GetString().Should().Be("Erste");
        stored.GetProperty("type").GetString().Should().Be("note");

        // ...while the indexed copy is normalised, so the filter still matches in any case.
        (await NestedAnnotationHitsAsync(("title", "erste"))).Should().Be(1, "lowercase should match");
        (await NestedAnnotationHitsAsync(("title", "ERSTE"))).Should().Be(1, "uppercase should match");
    }

    [Test]
    public async Task An_entry_is_searchable_by_the_things_the_old_index_searched()
    {
        (await FreeTextHitsAsync("01.02", "code.text")).Should().Be(1, "the code should match");
        (await FreeTextHitsAsync("Bevölkerung", "name.de")).Should().Be(1, "the whole name should match");
        (await FreeTextHitsAsync("Beschreibung", "description.de")).Should().Be(1, "the description should match");
    }

    [Test]
    public async Task Partial_input_matches_the_ngram_copy_and_a_whole_word_does_not()
    {
        // The ngram copy holds 2-3 character grams and is searched with a non-ngram analyser, so it is
        // what makes typing part of a name work...
        (await FreeTextHitsAsync("bev", "name.de.ngram")).Should().Be(1, "partial input should match");

        // ...and it cannot match a whole word, which is why the query has to search the plain field as
        // well. Lucene's boost table lists only the ngram field, and following that alone would have
        // left a full-name search finding nothing.
        (await FreeTextHitsAsync("Bevölkerung", "name.de.ngram")).Should().Be(0);
    }

    [Test]
    public async Task The_real_query_builder_finds_an_entry_by_the_things_people_type()
    {
        (await SearchHitsAsync("01.02")).Should().Be(1, "the whole code");
        (await SearchHitsAsync("01.0")).Should().Be(1, "the start of a code");
        (await SearchHitsAsync("Bevölkerung")).Should().Be(1, "the whole name");
        (await SearchHitsAsync("bev")).Should().Be(1, "part of the name");
        (await SearchHitsAsync("Bevölkerunk")).Should().Be(1, "a name with a typo in it");
        (await SearchHitsAsync("Beschreibung")).Should().Be(1, "a word from the description");

        // Annotation content, which the index this replaces could never return a hit for.
        (await SearchHitsAsync("Achtung")).Should().Be(1, "text on an annotation");
        (await SearchHitsAsync("warning")).Should().Be(1, "an annotation type");

        (await SearchHitsAsync("Wetterdaten")).Should().Be(0, "something absent");
    }

    [Test]
    public async Task A_search_is_confined_to_its_own_concept()
    {
        // The scope of the question, not a filter the caller may forget: another concept's entry with
        // the same code must not appear.
        (await SearchHitsAsync("01.02", concept: Guid.NewGuid())).Should().Be(0);
    }

    [Test]
    public async Task Every_term_has_to_match_but_not_all_in_one_field()
    {
        (await SearchHitsAsync("01.02 Bevölkerung")).Should().Be(1, "a code and a name together");
        (await SearchHitsAsync("01.02 Wetterdaten")).Should().Be(0, "one term matching is not enough");
    }

    [Test]
    public async Task A_configured_filter_requires_one_annotation_to_satisfy_all_of_it()
    {
        var sameAnnotation = new CodeListSearchFilter
        {
            All =
            [
                new CodeListAnnotationCriterion
                {
                    Type = "note",
                    Property = CodeListAnnotationProperty.Title,
                    Value = "Erste",
                },
            ],
        };

        (await SearchHitsAsync(null, filter: sameAnnotation)).Should().Be(1);

        var acrossTwoAnnotations = new CodeListSearchFilter
        {
            All =
            [
                new CodeListAnnotationCriterion
                {
                    Type = "note",
                    Property = CodeListAnnotationProperty.Title,
                    Value = "Zweite",
                },
            ],
        };

        // "Zweite" belongs to the warning, not the note. The entry has both parts but no single
        // annotation has both, so it must not match.
        (await SearchHitsAsync(null, filter: acrossTwoAnnotations)).Should().Be(0);
    }

    [Test]
    public async Task A_filter_on_text_matches_that_annotations_own_text()
    {
        var filter = new CodeListSearchFilter
        {
            All =
            [
                new CodeListAnnotationCriterion
                {
                    Type = "warning",
                    Text = new MultiLanguageModel { De = "Achtung" },
                },
            ],
        };

        (await SearchHitsAsync(null, filter: filter)).Should().Be(1);
    }

    private async Task<int> SearchHitsAsync(
        string? query,
        Guid? concept = null,
        CodeListSearchFilter? filter = null)
    {
        var body = CodeListQueryBuilder.BuildSearchBody(
            concept ?? ConceptId, query, "de", filter, from: 0, size: 20);

        return await TotalAsync(body);
    }
    private async Task<int> NestedAnnotationHitsAsync(params (string Field, string Value)[] criteria)
    {
        var must = criteria
            .Select(x => new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?> { [$"annotations.{x.Field}"] = x.Value },
            })
            .ToArray();

        var body = new Dictionary<string, object?>
        {
            ["size"] = 0,
            ["query"] = new Dictionary<string, object?>
            {
                ["nested"] = new Dictionary<string, object?>
                {
                    ["path"] = "annotations",
                    ["query"] = new Dictionary<string, object?>
                    {
                        ["bool"] = new Dictionary<string, object?> { ["must"] = must },
                    },
                },
            },
        };

        return await TotalAsync(body);
    }

    private async Task<int> FreeTextHitsAsync(string query, string field)
    {
        var body = new Dictionary<string, object?>
        {
            ["size"] = 0,
            ["query"] = new Dictionary<string, object?>
            {
                ["match"] = new Dictionary<string, object?> { [field] = query },
            },
        };

        return await TotalAsync(body);
    }

    private async Task<int> TotalAsync(Dictionary<string, object?> body)
    {
        var response = await _client.PostAsync($"/{Index}/_search", JsonContent.Create(body));
        await ShouldSucceedAsync(response, "search");

        return (await ReadJsonAsync(response)).GetProperty("hits").GetProperty("total")
            .GetProperty("value").GetInt32();
    }

    private async Task IndexAsync(CodeListIndexDocument entry)
    {
        var (id, document) = CodeListDocumentFactory.Build(entry);

        await ShouldSucceedAsync(
            await _client.PutAsync($"/{Index}/_doc/{id}?refresh=true", JsonContent.Create(document)),
            "index entry");
    }

    private async Task<JsonElement> GetJsonAsync(string path)
    {
        var response = await _client.GetAsync(path);
        await ShouldSucceedAsync(response, $"GET {path}");
        return await ReadJsonAsync(response);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();

    private static async Task ShouldSucceedAsync(HttpResponseMessage response, string what)
    {
        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"{what} failed with {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
