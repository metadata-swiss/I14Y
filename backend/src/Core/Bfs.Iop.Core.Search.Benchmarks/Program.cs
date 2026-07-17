using System.Diagnostics;
using System.Text;
using System.Text.Json;

// -----------------------------------------------------------------------------------------------
// Elasticsearch catalog-search benchmark (PoC).
//
// Measures, against a local Elasticsearch (docker-compose up -d), the metrics we compare to Lucene:
//   * indexing throughput (docs/sec) via _bulk, at several corpus sizes
//   * retrieval latency (p50/p95) for a representative query set
//   * RAM (JVM heap + OS memory) via _nodes/stats
//
// The Lucene baseline is measured separately by running the API in Lucene mode (LuceneHostedService
// logs the build time) and hitting /api/search with the same query set — see
// docs/search/elasticsearch-overview.md ("Benchmark method").
//
// Usage:
//   dotnet run -- [esUri] [sizes]
//   dotnet run                          # http://localhost:9200, sizes 1000,10000,100000
//   dotnet run -- http://localhost:9200 1000,10000
// -----------------------------------------------------------------------------------------------

var esUri = args.Length > 0 ? args[0].TrimEnd('/') : "http://localhost:9200";
var sizes = (args.Length > 1 ? args[1] : "1000,10000,100000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(int.Parse)
    .ToArray();

const string Index = "catalog_bench";
const int QueryRepeats = 200;

using var http = new HttpClient { BaseAddress = new Uri(esUri), Timeout = TimeSpan.FromMinutes(5) };

Console.WriteLine($"Elasticsearch benchmark against {esUri}");
Console.WriteLine(new string('=', 60));

if (!await PingAsync())
{
    Console.Error.WriteLine($"Cannot reach Elasticsearch at {esUri}. Run `docker compose up -d` first.");
    return 1;
}

var languages = new[] { "de", "en", "fr", "it", "rm" };
var themes = new[] { "01", "02", "03", "04", "05" };
var types = new[] { "Dataset", "DataService", "Concept", "MappingTable", "PublicService" };
// weight distribution mirrors RegistrationStatusToWeight (85..110)
var weights = new[] { 85, 90, 95, 98, 100, 102, 105, 110 };

foreach (var size in sizes)
{
    Console.WriteLine();
    Console.WriteLine($"--- Corpus size: {size:N0} documents ---");

    await RecreateIndexAsync();

    var indexSeconds = await BulkIndexAsync(size);
    Console.WriteLine($"Indexing time      : {indexSeconds:F2} s  ({size / indexSeconds:N0} docs/sec)");

    await http.PostAsync($"/{Index}/_refresh", null);

    var queries = new (string Label, string? Text)[]
    {
        ("single term", "daten"),
        ("two terms", "statistik daten"),
        ("partial/substring", "stat"),
        ("phrase", "\"offene daten\""),
        ("browse (no text)", null),
    };

    foreach (var (label, text) in queries)
    {
        var (p50, p95) = await MeasureQueryAsync(text);
        Console.WriteLine($"Retrieval {label,-20}: p50 {p50,6:F1} ms   p95 {p95,6:F1} ms");
    }

    await ReportMemoryAsync();
}

Console.WriteLine();
Console.WriteLine("Done. Compare these numbers to the Lucene baseline (see docs/search/elasticsearch-overview.md).");
return 0;

// --------------------------------------------------------------------------------------------

async Task<bool> PingAsync()
{
    try
    {
        var r = await http.GetAsync("/_cluster/health");
        return r.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

async Task RecreateIndexAsync()
{
    await http.DeleteAsync($"/{Index}");

    var mapping = new
    {
        settings = new
        {
            analysis = new
            {
                filter = new Dictionary<string, object>
                {
                    ["ngram_2_3"] = new { type = "ngram", min_gram = 2, max_gram = 3 },
                },
                analyzer = new Dictionary<string, object>
                {
                    ["i14y_text"] = new { type = "custom", tokenizer = "standard", filter = new[] { "lowercase", "asciifolding" } },
                    ["i14y_ngram"] = new { type = "custom", tokenizer = "standard", filter = new[] { "lowercase", "asciifolding", "ngram_2_3" } },
                    ["i14y_ngram_search"] = new { type = "custom", tokenizer = "standard", filter = new[] { "lowercase", "asciifolding" } },
                },
            },
        },
        mappings = new
        {
            properties = new Dictionary<string, object>
            {
                ["title"] = MultiLang(),
                ["description"] = MultiLang(),
                ["type"] = new { type = "keyword" },
                ["themes"] = new { type = "keyword" },
                ["registrationStatus"] = new { type = "integer" },
                ["registrationStatusWeight"] = new { type = "integer" },
                ["publicationLevel"] = new { type = "integer" },
            },
        },
    };

    var resp = await http.PutAsync($"/{Index}", JsonContent(mapping));
    resp.EnsureSuccessStatusCode();

    static object MultiLang() => new
    {
        properties = new Dictionary<string, object>
        {
            ["de"] = TextWithNgram(),
            ["en"] = TextWithNgram(),
            ["fr"] = TextWithNgram(),
            ["it"] = TextWithNgram(),
            ["rm"] = TextWithNgram(),
        },
    };

    static object TextWithNgram() => new
    {
        type = "text",
        analyzer = "i14y_text",
        fields = new Dictionary<string, object>
        {
            ["ngram"] = new { type = "text", analyzer = "i14y_ngram", search_analyzer = "i14y_ngram_search" },
        },
    };
}

async Task<double> BulkIndexAsync(int size)
{
    const int batch = 2000;
    var rnd = new Random(12345);
    var words = new[] { "daten", "statistik", "offene", "register", "gemeinde", "kanton", "bevölkerung", "wirtschaft", "umwelt", "verkehr" };
    var sw = Stopwatch.StartNew();

    for (var start = 0; start < size; start += batch)
    {
        var count = Math.Min(batch, size - start);
        var sb = new StringBuilder(count * 256);

        for (var i = 0; i < count; i++)
        {
            var id = start + i;
            var title = $"{Pick(words)} {Pick(words)} {id}";
            var description = $"{Pick(words)} {Pick(words)} {Pick(words)}";
            var weight = weights[rnd.Next(weights.Length)];

            sb.Append("{\"index\":{\"_id\":\"").Append(id).Append("\"}}\n");
            var doc = new Dictionary<string, object?>
            {
                ["title"] = new Dictionary<string, object?> { ["de"] = title, ["en"] = title },
                ["description"] = new Dictionary<string, object?> { ["de"] = description },
                ["type"] = types[rnd.Next(types.Length)],
                ["themes"] = new[] { themes[rnd.Next(themes.Length)] },
                ["registrationStatusWeight"] = weight,
                ["publicationLevel"] = 40, // Public
            };
            sb.Append(JsonSerializer.Serialize(doc)).Append('\n');
        }

        using var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/x-ndjson");
        var resp = await http.PostAsync($"/{Index}/_bulk", content);
        resp.EnsureSuccessStatusCode();
    }

    sw.Stop();
    return sw.Elapsed.TotalSeconds;

    string Pick(string[] arr) => arr[rnd.Next(arr.Length)];
}

async Task<(double P50, double P95)> MeasureQueryAsync(string? text)
{
    var body = BuildQuery(text);
    var samples = new List<double>(QueryRepeats);

    // warm-up
    for (var i = 0; i < 5; i++)
    {
        await http.PostAsync($"/{Index}/_search", JsonContent(body));
    }

    for (var i = 0; i < QueryRepeats; i++)
    {
        var sw = Stopwatch.StartNew();
        var resp = await http.PostAsync($"/{Index}/_search", JsonContent(body));
        resp.EnsureSuccessStatusCode();
        await resp.Content.ReadAsStringAsync();
        sw.Stop();
        samples.Add(sw.Elapsed.TotalMilliseconds);
    }

    samples.Sort();
    return (Percentile(samples, 50), Percentile(samples, 95));
}

object BuildQuery(string? text)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return new { size = 20, query = new { match_all = new { } } };
    }

    var fields = new List<string>();
    var ngramFields = new List<string>();
    foreach (var lang in languages)
    {
        fields.Add($"title.{lang}^2.0");
        fields.Add($"description.{lang}^1.5");
        ngramFields.Add($"title.{lang}.ngram^2.0");
        ngramFields.Add($"description.{lang}.ngram^1.5");
    }

    return new
    {
        size = 20,
        query = new
        {
            function_score = new
            {
                query = new
                {
                    @bool = new
                    {
                        should = new object[]
                        {
                            new { multi_match = new { query = text, type = "best_fields", fields = fields.ToArray() } },
                            new { multi_match = new { query = text, type = "best_fields", fields = ngramFields.ToArray(), boost = 0.75 } },
                        },
                        minimum_should_match = 1,
                    },
                },
                field_value_factor = new { field = "registrationStatusWeight", factor = 0.01, missing = 100 },
                boost_mode = "multiply",
            },
        },
    };
}

async Task ReportMemoryAsync()
{
    var json = await http.GetStringAsync("/_nodes/stats/jvm,os");
    using var doc = JsonDocument.Parse(json);
    var nodes = doc.RootElement.GetProperty("nodes");
    foreach (var node in nodes.EnumerateObject())
    {
        var jvm = node.Value.GetProperty("jvm").GetProperty("mem");
        var heapMb = jvm.GetProperty("heap_used_in_bytes").GetInt64() / (1024.0 * 1024.0);
        var os = node.Value.GetProperty("os").GetProperty("mem");
        var osUsedMb = os.GetProperty("used_in_bytes").GetInt64() / (1024.0 * 1024.0);
        Console.WriteLine($"RAM                : JVM heap used {heapMb:N0} MB   OS mem used {osUsedMb:N0} MB");
    }
}

static double Percentile(List<double> sorted, int percentile)
{
    if (sorted.Count == 0)
    {
        return 0;
    }

    var rank = (percentile / 100.0) * (sorted.Count - 1);
    var low = (int)Math.Floor(rank);
    var high = (int)Math.Ceiling(rank);
    return low == high ? sorted[low] : sorted[low] + (rank - low) * (sorted[high] - sorted[low]);
}

static HttpContent JsonContent(object body) =>
    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
