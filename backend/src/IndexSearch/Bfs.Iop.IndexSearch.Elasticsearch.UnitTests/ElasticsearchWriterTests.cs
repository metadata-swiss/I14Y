using System.Net.Http;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// The writers exist so a rebuild can report what the index actually took. A bulk request answers 200
// even when it rejected documents one at a time, so that claim can only be checked against a real
// server: a stub would return whatever we told it to.
[TestFixture]
[Explicit("Needs a live Elasticsearch. docker compose up -d elasticsearch")]
[Category("Integration")]
public class ElasticsearchWriterTests
{
    private const string CatalogIndex = "i14y-catalog-writertest";

    private HttpClient _client = null!;
    private ElasticsearchBulkWriter _bulk = null!;
    private ElasticsearchCatalogIndexWriter _writer = null!;

    [SetUp]
    public async Task CreateIndexAsync()
    {
        var uri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH") ?? "http://localhost:9200";
        var host = new Uri(uri).Host;

        if (host is not ("localhost" or "127.0.0.1" or "[::1]"))
        {
            Assert.Fail($"Refusing to run against '{host}': this fixture deletes indices. Use a local node.");
        }

        _client = new HttpClient { BaseAddress = new Uri(uri), Timeout = TimeSpan.FromSeconds(30) };

        await _client.DeleteAsync($"/{CatalogIndex}");
        await _client.PutAsync(
            $"/{CatalogIndex}",
            new StringContent(CatalogIndexMapping.BuildCreateIndexJson(), Encoding.UTF8, "application/json"));

        var names = new IndexNames(Options.Create(new ElasticsearchOptions
        {
            CatalogIndexName = CatalogIndex,
            CodeListIndexName = "i14y-codelist-writertest",
        }));

        _bulk = new ElasticsearchBulkWriter(_client, NullLogger<ElasticsearchBulkWriter>.Instance);
        _writer = new ElasticsearchCatalogIndexWriter(_bulk, new IndexWriteTarget(names));
    }

    [TearDown]
    public void Dispose() => _client?.Dispose();

    [Test]
    public async Task Writing_documents_returns_how_many_the_index_took()
    {
        var written = await _writer.WriteAsync([Document("A"), Document("B")]);

        written.Should().Be(2);
        (await CountAsync()).Should().Be(2, "and they should really be there");
    }

    [Test]
    public async Task A_document_the_index_rejects_is_not_counted_as_written()
    {
        // One good document and one whose registrationStatusWeight is not a number. Elasticsearch
        // answers 200 for the request and 400 for that single item, which is exactly the case that
        // made counting the batch size a lie.
        var good = CatalogDocumentFactory.Build(Document("A"));
        var bad = CatalogDocumentFactory.Build(Document("B"));
        bad.Body["registrationStatusWeight"] = "not a number";

        var written = await _bulk.IndexAsync(CatalogIndex, [good, bad], CancellationToken.None);

        written.Should().Be(1, "the rejected document must not be counted");
        (await CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task An_index_name_that_cannot_exist_reports_nothing_written()
    {
        var document = CatalogDocumentFactory.Build(Document("A"));

        // Elasticsearch answers 200 even for an index name it refuses outright, putting the rejection
        // in the item status. So a misconfigured index name surfaces as "nothing landed" and not as a
        // silent success — which is only true because the count comes from the items.
        var written = await _bulk.IndexAsync("_illegal", [document], CancellationToken.None);

        written.Should().Be(0);
    }

    [Test]
    public async Task A_node_that_cannot_be_reached_fails_the_batch_instead_of_reporting_nothing_written()
    {
        using var unreachable = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:9"),
            Timeout = TimeSpan.FromSeconds(5),
        };

        var writer = new ElasticsearchBulkWriter(unreachable, NullLogger<ElasticsearchBulkWriter>.Instance);

        // A rebuild has to tell "the index was unreachable" from "the batch was empty". Reporting 0
        // for both would count a whole lost batch as a successful no-op.
        var write = async () => await writer.IndexAsync(
            CatalogIndex,
            [CatalogDocumentFactory.Build(Document("A"))],
            CancellationToken.None);

        await write.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task Deleting_removes_the_documents_it_names_and_leaves_the_rest()
    {
        var keep = Document("A");
        var remove = Document("B");

        await _writer.WriteAsync([keep, remove]);

        await _writer.DeleteAsync([remove.Id]);
        await _client.PostAsync($"/{CatalogIndex}/_refresh", content: null);

        (await CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task An_empty_batch_costs_no_request()
    {
        // The rebuild's last batch is routinely empty, and a bulk body with no lines is a 400.
        (await _writer.WriteAsync([])).Should().Be(0);

        var delete = async () => await _writer.DeleteAsync([]);
        await delete.Should().NotThrowAsync();
    }

    private static CatalogIndexDocument Document(string suffix) => new()
    {
        Id = Guid.Parse($"0000000{suffix.Length}-0000-0000-0000-{suffix.PadLeft(12, '0')}"),
        Type = SearchResourceType.Dataset,
        Identifiers = [$"BFS-{suffix}"],
        PublicationLevel = PublicationLevel.Public,
        RegistrationStatus = RegistrationStatus.Standard,
        CreationType = CreationType.Manual,
        Title = new MultiLanguageModel { De = $"Datensatz {suffix}" },
    };

    private async Task<int> CountAsync()
    {
        await _client.PostAsync($"/{CatalogIndex}/_refresh", content: null);

        var response = await _client.GetAsync($"/{CatalogIndex}/_count");
        var payload = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(payload).RootElement.GetProperty("count").GetInt32();
    }
}
