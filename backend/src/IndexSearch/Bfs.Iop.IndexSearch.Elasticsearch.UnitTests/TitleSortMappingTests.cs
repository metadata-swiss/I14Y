using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(ElasticsearchIndexProvisioner))]
internal sealed class TitleSortMappingTests
{
    private const string Catalog = "i14y-catalog";
    private const string CodeList = "i14y-codelist";

    [Test]
    public async Task An_existing_catalog_index_gets_the_title_sort_mapping_before_anything_writes_to_it()
    {
        var handler = new StubHandler(HttpStatusCode.OK);

        await Provisioner(handler).CreateIfMissingAsync();

        var (path, body) = handler.MappingUpdates.Single();

        path.Should().Be($"/{Catalog}/_mapping");

        using var mapping = JsonDocument.Parse(body);
        var english = mapping.RootElement.GetProperty("properties").GetProperty(EsCatalogFields.TitleSort)
            .GetProperty("properties").GetProperty("en");

        english.GetProperty("type").GetString().Should().Be("keyword");
    }

    [Test]
    public async Task The_code_list_index_mapping_is_left_alone()
    {
        var handler = new StubHandler(HttpStatusCode.OK);

        await Provisioner(handler).CreateIfMissingAsync();

        handler.MappingUpdates.Should().NotContain(x => x.Path.Contains(CodeList, StringComparison.Ordinal));
    }

    [Test]
    public async Task A_rejected_title_sort_mapping_does_not_stop_the_startup()
    {
        var handler = new StubHandler(HttpStatusCode.BadRequest);

        var startup = async () => await Provisioner(handler).CreateIfMissingAsync();

        await startup.Should().NotThrowAsync();
    }

    private static ElasticsearchIndexProvisioner Provisioner(StubHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://elasticsearch.test") };

        var names = new IndexNames(Options.Create(new ElasticsearchOptions
        {
            CatalogIndexName = Catalog,
            CodeListIndexName = CodeList,
        }));

        return new ElasticsearchIndexProvisioner(client, names, NullLogger<ElasticsearchIndexProvisioner>.Instance);
    }

    private sealed class StubHandler(HttpStatusCode mappingStatus) : HttpMessageHandler
    {
        private readonly ConcurrentQueue<(string Path, string Body)> _mappingUpdates = new();

        public IReadOnlyList<(string Path, string Body)> MappingUpdates => [.. _mappingUpdates];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Head)
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (request.Method == HttpMethod.Put && path.EndsWith("/_mapping", StringComparison.Ordinal))
            {
                _mappingUpdates.Enqueue((path, await request.Content!.ReadAsStringAsync(cancellationToken)));

                return new HttpResponseMessage(mappingStatus) { Content = new StringContent("{}") };
            }

            throw new InvalidOperationException($"Unexpected request {request.Method} {path}.");
        }
    }
}
