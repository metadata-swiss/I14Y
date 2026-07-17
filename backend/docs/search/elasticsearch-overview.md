# Elasticsearch for a .NET dev — overview + PoC guide

A practical introduction to Elasticsearch (ES) for someone who knows the
[current Lucene setup](lucene-current-architecture.md), plus how the I14Y ES PoC is wired and how to
run/measure it.

> **Fun fact that makes ES easy to reason about:** Elasticsearch is *built on Lucene*. Every ES shard
> **is** a Lucene index. So the analyzers, tokenizers and scoring you already know still apply — ES just
> wraps them behind a JSON/REST API, adds clustering, and gives you real operational tooling. Moving
> from Lucene.NET to ES is mostly *"stop hand-writing the plumbing; describe it in JSON instead."*

## Lucene.NET → Elasticsearch cheat sheet

| Lucene.NET (today) | Elasticsearch equivalent |
|---|---|
| Embedded library, in-process | Separate **server**; talk to it over REST/JSON (port 9200) via `Elastic.Clients.Elasticsearch` |
| `Directory` (FSDirectory/RAM) | Cluster → index → shards (each shard is a Lucene index), managed by ES |
| `Document` + `Field` | A JSON document + an index **mapping** (declares field types/analyzers) |
| `PerFieldAnalyzerWrapper` + custom `Analyzer` classes | `settings.analysis` JSON: `tokenizer`, `filter`, `analyzer`. Multilingual via sub-fields `title.de`, `title.fr` |
| `TextField { Boost = 2.0 }` at index time | Boost at **query time**: `multi_match` with `fields: ["title.de^2","keyword.de^1.75", …]` |
| `PartialTermMultiFieldQueryParser` (`*term*` @ 0.75) | A second `multi_match` over `.ngram` sub-fields in a `bool.should`, `boost: 0.75` |
| `RegistrationStatusBoostQuery : CustomScoreQuery` | `function_score` + `field_value_factor` on `registrationStatusWeight` (factor 0.01 → ×weight/100) |
| Taxonomy index + `DrillSideways` | `aggregations` (+ `post_filter` for full drill-sideways) |
| Auth `BooleanFilter` | `bool.filter` clause |
| `IndexWriter.UpdateDocument(Term(Id))` | `index` / `_bulk` API keyed on `_id` |
| Startup `RebuildIndex()` | One `_bulk` load (+ optional alias swap for zero-downtime reindex) |
| Query every time opens a `DirectoryReader` | ES manages readers; near-real-time by default (`_refresh`) |
| **No ops tooling** | **Kibana Dev Tools**, `_cat` APIs, `_explain`, `_nodes/stats`, `_analyze` |

## Run Elasticsearch locally

There was no Docker in this repo before; the PoC adds a root **`docker-compose.yml`** with a single-node
ES 8.x (security disabled — dev only) and Kibana.

```bash
docker compose up -d                    # start ES (9200) + Kibana (5601)
curl http://localhost:9200/_cluster/health   # expect "status":"green"|"yellow"
# Kibana Dev Tools console: http://localhost:5601/app/dev_tools#/console
docker compose down                     # stop (keeps data)
docker compose down -v                  # stop + wipe the index
```

## How the PoC is wired into the backend

The PoC is **additive and behind a toggle** — the default stays Lucene, nothing existing changes.

```
Bfs.Iop.Core.Api/Startup.cs
   var engine = Configuration.GetValue<string>("Search:Engine");   // "Lucene" (default) | "Elasticsearch"
   engine == "Elasticsearch"
       ? services.AddElasticsearchSearch(Configuration)
       : services.AddLuceneSearch();
```

New project **`src/Core/Bfs.Iop.Core.Elasticsearch`** (mirrors the Lucene project):

| File | Role | Lucene counterpart |
|---|---|---|
| `ServiceCollectionExtensions.cs` | `AddElasticsearchSearch()` — registers the client, index service, builder, hosted service | `AddLuceneSearch()` |
| `ElasticsearchCatalogIndexService.cs` | Implements **`ICatalogIndexService`** (drop-in) | `CatalogIndexService` |
| `CatalogIndexMapping.cs` | Builds the create-index JSON (analysis + field mappings) | `PerFieldAnalyzerWrapper` + analyzers |
| `CatalogQueryBuilder.cs` | Builds query/aggregation JSON — **the ranking rules** | `BuildSearchQuery` + `RegistrationStatusBoostQuery` |
| `CatalogDocumentFactory.cs` | Model → JSON document | `BuildDocument` / `GetDocumentFrom*` |
| `ElasticsearchCatalogIndexBuilderService.cs` | Full (re)build in batches | `CatalogIndexBuilderService` |
| `ElasticsearchHostedService.cs` | Create index + build at startup | `LuceneHostedService` |
| `EsCatalogFields.cs` | Field-name constants | `LuceneFields` |

Because the command handlers depend only on `ICatalogIndexService`, **they are unchanged**.

### The client (DI registration pattern)

```csharp
services.AddOptions<ElasticsearchOptions>()
        .Bind(configuration.GetSection(ElasticsearchOptions.SectionName));   // "Elasticsearch" section

services.AddSingleton(sp =>
{
    var o = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
    var settings = new ElasticsearchClientSettings(new Uri(o.Uri));
    // basic auth optional; the local dev node has security disabled
    return new ElasticsearchClient(settings);
});
```

> **Implementation note:** the PoC drives ES with **raw JSON over the client's low-level transport**
> (`client.Transport.RequestAsync<StringResponse>(HttpMethod.POST, "/catalog/_search", PostData.String(json))`).
> This keeps us independent of the strongly-typed query DSL (which changes a lot between client versions)
> and makes the generated queries trivial to copy-paste into Kibana Dev Tools for debugging. A production
> implementation might prefer the typed API for compile-time safety.

### Config (`appsettings.Development.json`)

```jsonc
"Search":        { "Engine": "Lucene" },          // switch to "Elasticsearch" for the PoC
"Elasticsearch": { "Uri": "http://localhost:9200", "CatalogIndexName": "catalog", "RecreateIndexOnStartup": true }
```

### How the ranking rules translate

The whole search body the PoC builds (for a text query) looks like this:

```jsonc
{
  "query": {
    "bool": {
      "must": [{
        "function_score": {                              // registration-status ranking …
          "query": {
            "bool": {
              "should": [
                { "multi_match": {                        // exact tokens, per-field boosts
                    "query": "…", "type": "best_fields",
                    "fields": ["title.de^2","name.de^2","keyword.de^1.75","description.de^1.5","identifier^1.5", …] } },
                { "multi_match": {                        // partial (ngram), dampened
                    "query": "…", "fields": ["title.de.ngram^2", …], "boost": 0.75 } }
              ],
              "minimum_should_match": 1
            }
          },
          "field_value_factor": { "field": "registrationStatusWeight", "factor": 0.01, "missing": 100 },
          "boost_mode": "multiply"                        // score × (weight/100)  → PreferredStandard ×1.10 …
        }
      }],
      "filter": [                                         // facets + numeric filters + auth
        { "terms": { "themes": ["01"] } },
        { "bool": { "should": [ { "term": { "publicationLevel": 40 } },
                                { "terms": { "publisherIdentifier": ["…"] } } ], "minimum_should_match": 1 } }
      ]
    }
  }
}
```

The `function_score` wrapper is added **only when there is a text query** — exactly like the Lucene
`RegistrationStatusBoostQuery` gating.

> **"More reused concepts at the top"** is not implemented in either engine today. In ES it would be a
> second `function_score` function — e.g. `field_value_factor` on an indexed `reuseCount` (data available
> from `RelationsCountService`), or a `script_score`. This is a natural extension once the PoC lands.

## Kibana Dev Tools — quick experiments

```
GET catalog/_count
GET catalog/_mapping

# see how a German title is analysed (stemming/stopwords/ngram)
POST catalog/_analyze
{ "analyzer": "i14y_de", "text": "Offene Verwaltungsdaten" }

# why did this doc score what it scored?
GET catalog/_explain/<docId>
{ "query": { "match": { "title.de": "daten" } } }

GET _nodes/stats/jvm,os      # RAM: heap_used_in_bytes / os.mem.used_in_bytes
GET _cat/indices?v
```

## Benchmark method (ES vs Lucene)

Project **`src/Core/Bfs.Iop.Core.Search.Benchmarks`** measures the ES side against synthetic data (no
Postgres needed):

```bash
docker compose up -d
cd src/Core/Bfs.Iop.Core.Search.Benchmarks
dotnet run                                  # sizes 1k / 10k / 100k
dotnet run -- http://localhost:9200 1000,10000
```

It reports **indexing throughput (docs/sec)**, **retrieval p50/p95** for a query set (single term, two
terms, partial, phrase, browse), and **RAM (JVM heap + OS mem)** from `_nodes/stats`.

**Lucene baseline** (in-process, so measured differently):
1. Run the API with `Search:Engine=Lucene`. `LuceneHostedService` logs the index build time — that's the
   indexing number.
2. Hit `GET /api/search?query=…` with the same query set; time the responses (p50/p95).
3. RAM = the API process working-set delta while the Lucene index is open (Lucene is embedded, so there
   is no separate process — compare "API RAM (Lucene)" vs "API RAM (ES mode) + ES container RAM").

### Results (fill in after running)

| Metric | Corpus | Lucene | Elasticsearch |
|---|---|---|---|
| Indexing (docs/sec) | 1k / 10k / 100k | _tbd_ | _tbd_ |
| Retrieval p50 (ms) | single term | _tbd_ | _tbd_ |
| Retrieval p95 (ms) | single term | _tbd_ | _tbd_ |
| RAM | 100k docs | _tbd_ | _tbd_ |

### Ease of use — qualitative scorecard (fill in / adjust)

| Dimension | Lucene.NET | Elasticsearch |
|---|---|---|
| Setup | NuGet only, zero infra | Needs a server/container (docker-compose here) |
| Analyzers/queries/facets | Hand-written C# (100s of lines) | Declarative JSON mapping + query bodies |
| Debuggability | Attach debugger; `Explain` in logs | Kibana `_explain` / `_analyze`, live |
| Ops (scaling, monitoring, backup) | You build it | Built-in (cluster, snapshots, Kibana) |
| Upgrades | Beta package pinned to LUCENE_48 | Versioned server + client; reindex on major upgrades |
| Multi-language | Custom `LanguageDependentAnalyzer` | Built-in language analyzers + your custom ones |

## CodeList-entry search

The second index (values inside one concept's code list) is also ported, so ES mode is functionally
complete. It has a very different ranking scheme from the catalog:

- **Field boosts:** Code **20**, Name-ngram **16**, Description **12**, annotation fields **8**.
- **Query:** the query is split into terms; **AND across terms, OR across fields** (terms ≥ 2 chars).
  Code → `prefix`, Name-ngram → `match` with `minimum_should_match: "60%"`, Description → `match` with
  `fuzziness: AUTO`, annotations → a `nested` `multi_match`.
- **`nested` instead of block-join.** Lucene indexes each entry as a parent doc + child annotation docs
  joined at query time (`JoinUtil`). In ES each entry is **one document with a `nested` `annotations`
  array**; annotation search and filters are `nested` queries — simpler and no join machinery.
- **Same result flow:** query ES → entry ids + scores → hydrate full models from the DB
  (`GetCodeListEntriesByIds`) → build ancestor paths. Reused verbatim from the Lucene service.
- Files live under `Bfs.Iop.Core.Elasticsearch/CodeList/`; the index is `codelist`
  (`Elasticsearch:CodeListIndexName`). Endpoint: `GET api/concepts/{id}/codelist-entries/search`.

## PoC scope & known limitations

- **Facet counts are approximated.** The PoC computes aggregations over the text query + authorization
  only (not the selected facets), which mimics drill-sideways at the "no facet selected" level. Full
  per-dimension drill-sideways (`post_filter` + per-facet filtered aggs) is a follow-up.
- **Numeric-facet values** (registration status / publication level / concept type) bucket on the integer
  value; the count command handler may expect enum names for perfect parity — a small mapping follow-up.
- **Contracts still live in the Lucene project.** For the drop-in to work, the ES project references
  `Bfs.Iop.Core.Lucene` to reuse `ICatalogIndexService` and the result records. A production move would
  extract these into `Bfs.Iop.Core.Abstractions`.
- The docker-compose stack is **dev-only** (single node, no auth/TLS). Production would use a managed
  cluster and put connection settings in Azure App Config / Key Vault.

## Verification checklist

1. `docker compose up -d` → `_cluster/health` green/yellow; Kibana reachable.
2. API with `Search:Engine=Elasticsearch` builds the `catalog` index; `GET catalog/_count` matches the
   Lucene doc count.
3. Search behaves correctly — see **Testing the search** below.
4. `dotnet run` in the benchmark project produces the numbers for the results table above.
5. With `Search:Engine=Lucene` (default), everything behaves exactly as before.

## Testing the search

Matching doc counts (step 2) only proves the index *built*. To prove it *behaves*, test in layers —
cheapest and most deterministic first. Both engines index from the **same Postgres DB**, so a Lucene↔ES
A/B is apples-to-apples: flip `Search:Engine` and restart.

### Layer 1 — ranking logic in isolation (Kibana, deterministic)

Index a tiny known corpus into a scratch index and assert the order. A failure here points straight at
`CatalogQueryBuilder` / `CatalogIndexMapping` — no DB or .NET involved.

```
# 6 docs: same title token "daten", different registration-status weights, plus a description-only match.
POST catalog/_bulk
{"index":{"_id":"1"}}
{"title":{"de":"daten portal"},"registrationStatusWeight":110,"publicationLevel":40,"type":"Dataset"}
{"index":{"_id":"2"}}
{"title":{"de":"daten portal"},"registrationStatusWeight":105,"publicationLevel":40,"type":"Dataset"}
{"index":{"_id":"3"}}
{"title":{"de":"daten portal"},"registrationStatusWeight":98,"publicationLevel":40,"type":"Dataset"}
{"index":{"_id":"4"}}
{"title":{"de":"daten portal"},"registrationStatusWeight":85,"publicationLevel":40,"type":"Dataset"}
{"index":{"_id":"5"}}
{"description":{"de":"offene daten"},"registrationStatusWeight":110,"publicationLevel":40,"type":"Dataset"}
{"index":{"_id":"6"}}
{"title":{"de":"statistik"},"registrationStatusWeight":110,"publicationLevel":40,"type":"Dataset"}
```

Run the query body from the "How the ranking rules translate" section above for `query = "daten"`.
**Expected:** `1 > 2 > 3 > 4` (status multiplier ×1.10 → ×0.85 on equal title matches), all above `5`
(title boost 2.0 beats description 1.5), and `6` absent. Use `GET catalog/_explain/2 {…}` to see the score
= title-match × (registrationStatusWeight × 0.01).

### Layer 2 — end-to-end via `GET /api/search`

Exercises the real path (command handler → `ICatalogIndexService` → ES) against the real corpus. One case
per business rule:

| Rule | Request | Expected |
|---|---|---|
| Field weight | `?query=<word in a title>` | Title/Name hits rank above description-only hits |
| Registration status | `?query=<common word>` | at equal textual relevance, PreferredStandard/Standard above Candidate/Retired |
| Partial/substring | `?query=stat` (prefix of "statistik") | substring hits returned, scored below exact-token hits |
| Multilingual | `?query=<fr word>&language=fr` vs no `language` | `language=fr` restricts to French fields; omitting it searches all 5 |
| Email special-case | `?query=<a contact email>` | the exact resource(s) with that email, not tokenised noise |
| Authorization | anonymous vs privileged token | anonymous sees only `PublicationLevel=Public`; InteroperabilityService/SwissDataSteward see all |
| Filters | `?query=daten&themes=<code>&types=Dataset` | results respect the filters; `total` drops accordingly |

Use ES `_explain` in Kibana on any surprising result to see the field-boost × status-factor breakdown.

### Layer 3 — A/B parity vs Lucene (the acceptance test)

1. Start with `Search:Engine=Lucene`; capture responses for the query set (result ids in order,
   `TotalCount`, and `api/search/count` buckets).
2. Restart with `Search:Engine=Elasticsearch`; capture the same.
3. Diff: total counts and result **sets** should match; top-N **ordering** should match for the ranking
   cases. Score *values* will differ (Lucene and ES scale scores differently) — only relative order
   matters. Facet buckets should match modulo the documented drill-sideways / numeric-value
   approximations.

A short PowerShell/`curl`+`jq` script (or an extension of the benchmark console app) can automate the
capture and diff.
