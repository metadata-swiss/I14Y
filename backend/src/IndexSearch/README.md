# IndexSearch

Elasticsearch-backed search, split out of IOP Core into its own service.

| Project | What it is |
| --- | --- |
| `Bfs.Iop.IndexSearch.Api` | The service: owns the Elasticsearch indexes, serves search, accepts index writes. |
| `Bfs.Iop.IndexSearch.ApiClient` | The typed HTTP client for this service. Knows nothing about its callers. |
| `../Core/Bfs.Iop.Core.IndexForwarding` | Core's side of the wire: implements the search ports by calling that client. |
| `../Search/Bfs.Iop.Search.Abstractions` | Engine-neutral search contracts. |
| `../Search/Bfs.Iop.Search.Elasticsearch` | The Elasticsearch implementation of those contracts. |

This is the only service that references a search engine. Everything else — IOP Core included —
reaches the index over HTTP, and IOP Core is the only caller: Admin and Partner go through it.

## How a change reaches the index

1. A user saves a resource. IOP Core writes to Postgres.
2. `ForwardingCatalogIndexService` queues the model on `IndexSearchDispatcher` — Core holds no index
   of its own, so this is the whole of its write path.
3. `IndexForwardingSenderHostedService` drains the queue off the request path and POSTs to
   `/api/Index/...`. The request carries the resource itself, so this service never reads it back.
4. The service queues the event, and a background worker writes the document to Elasticsearch.

Every step after the save is best-effort. A save never fails, blocks, or slows down because indexing
did — which is only acceptable because of the next section.

## Which rebuilds purge, and which do not

A full build is **upsert-only**: it indexes every row it reads from Postgres and deletes nothing. So a
document whose row was deleted, but whose de-index event was lost — routine, because the forward queue
is in-memory and does not survive a restart — only disappears when the index is *recreated*. The same
is true of a mapping change: `EnsureIndexAsync` creates an index only when it is absent.

| Path | Recreates? |
| --- | --- |
| Service startup | **Yes** — `Elasticsearch:RecreateIndexOnStartup`, `true` by default |
| `POST /api/Index/recreate` | **Yes** |
| `POST /api/Index/rebuild` | No |
| The scheduled rebuild | No, unless `IndexSearch:RecreateOnScheduledRebuild` is on |

Recreating drops the indexes *before* rebuilding, so from that moment until the build finishes every
search returns HTTP 200 with no results. `GET /health/ready` reports 503 for that window so the
platform can take the replica out of rotation — but only if the readiness probe is actually pointed at
it, which is Container App configuration, not something the deploy workflow sets. `GET /health` stays
200 throughout, deliberately: failing liveness would have the platform kill the container mid-build.

If a recreating build fails — most often an unreachable database — the indexes are left **empty**, not
stale. The error log says so explicitly; the recovery is `POST /api/Index/recreate` once the cause is
fixed.

## The full reindex is not optional

`IndexSearch:FullReindexIntervalHours` rebuilds both indexes from Postgres on a schedule. That is what
repairs a forward that was dropped on a full queue, a batch that failed, or a queue lost on restart.
Every failure path in this design assumes it runs. Setting it to `0` disables it, logs a warning, and
leaves `POST /api/Index/rebuild` as the only repair.

## Configuration

The service (`IndexSearch` section):

| Key | Notes |
| --- | --- |
| `Secret` | Required. Empty means every write is rejected — it fails closed, never open. |
| `QueueCapacity` | Pending events held in memory. Full ⇒ callers get 429. |
| `BatchSize` | Events collapsed into one reconcile pass. |
| `FullReindexIntervalHours` | See above. `0` disables. |
| `BuildIndexOnStartup` | Build once at startup after ensuring the indexes exist. |
| `RecreateOnScheduledRebuild` | Whether the scheduled rebuild purges too. Default `false` — see below. |

Plus an `Elasticsearch` section (`Uri`, `CatalogIndexName`, `CodeListIndexName`,
`RecreateIndexOnStartup`) and the same `postgres` settings IOP Core uses — the full index build reads
the same database.

IOP Core (`IndexSearchApiClient` section):

| Key | Notes |
| --- | --- |
| `BaseUrl` | **Required — Core fails at startup without it**, including an unreplaced `#{TOKEN}#`. It is Core's only path to an index, for reads as well as writes, so degrading quietly is not an option. |
| `Secret` | **Must equal the service's `IndexSearch:Secret`.** |

Those two keys are the whole of Core's index configuration. **The outbound queue is not
configurable**: capacity (10,000) and batch size (100) are constants on `IndexSearchDispatcher` and
`IndexForwardingSenderHostedService`, and the HTTP timeouts (30s writes, 60s reads) are hard-coded in
`AddIndexSearchApiClient` / `AddIndexSearchSearchClient`.

They used to be bound from an `IndexForwarding` configuration section, but that section was set in no
`appsettings*.json` anywhere, so the defaults were always the effective values — a knob that read as
tunable and was not. Making any of them configurable again is a deliberate change, not a
rediscovery.

> If the two secrets disagree, every forward is rejected with 403 and the index simply stops
> updating. Nothing else breaks, so the symptom is stale search results, not an error.

## Running locally

```bash
docker compose up -d
```

Starts Elasticsearch (`:9200`), Kibana (`:5601`) and this service (`:8003`). IOP Core is not in the
compose file — run it from the IDE and set `IndexSearchApiClient:BaseUrl` to `http://localhost:8003`
in `appsettings.Development.json`. Core will not start without it.

## Authorization

Search results are user-scoped, but the filtering happens **inside the Elasticsearch query**, from the
caller's JWT claims — not before it. Indexing deliberately bypasses authorization so the index holds
non-public resources too; otherwise an entitled user could never find their own unpublished work.

The consequence: this service's JWT configuration is security-critical. If `Keycloak`/`Eiam` is
misconfigured, token validation fails, agencies come back empty, and every caller silently drops to
public-only results — a 200 with missing rows, not an error.

## Facet counts (drill-sideways)

A facet count is a promise: *click me and you get this many results*. Keeping that promise means
counting each dimension with every selection applied **except its own** — otherwise the numbers
describe a catalogue the user is not looking at, and values can be offered that return nothing.

`BuildCountBody` implements this the Elasticsearch way: the base query carries text and authorization
only, and each dimension gets a `filter` aggregation holding the other dimensions' selections,
wrapping the terms aggregation that produces its buckets. One request, and the same semantics the
previous engine provided.

Two consequences worth knowing before changing this code:

- **Adding a facet means touching two places.** A dimension must appear in `_facetDimensions` *and*
  in `BuildFilterClausesByDimension`. Present in the first only, and its own selection is never
  excluded from its own count — drill-sideways silently degrades for that dimension alone.
- **`TotalDocumentsCount` is per dimension**, not the filtered result count: it is each filter
  aggregation's own `doc_count`. This reproduces the previous engine, where the count handler read
  the overall `TotalDocCount` from a single dimension's own-filter-removed total. It looks like a
  quirk because it is one; it was carried over deliberately so the headline number did not move
  during the migration. Changing it is a UI-visible change, not a cleanup.
