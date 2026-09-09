# IndexSearch — plan

Extract search and indexing out of `Bfs.Iop.Core.Api` (in-process Lucene) into a standalone
Elasticsearch-backed service, in granular PRs.

Status as of 2026-09-09: build 0 errors / 0 warnings, 665 tests passing, 130 of them IndexSearch.
PR #179 (`838b28a`, the catalog engine) is merged.

## Architecture

```
Contracts     -> DataAccess.Abstractions, Infrastructure.Security
Business      -> Contracts
Elasticsearch -> Contracts
Data          -> DataAccess, DataAccess.Abstractions, Core.LinkedData, Business, Contracts
Api           -> DataAccess.Relational, Data, Elasticsearch
```

The load-bearing property: **`Elasticsearch` and `Data` do not reference each other.** Both adapters
meet only at the interfaces in `Contracts`, and only `Api` knows both. That is what keeps the engine
testable and replaceable without a database. No `Bfs.Iop.Core` dependency anywhere in IndexSearch
except `Core.LinkedData`, which exists solely because the dataset structure flag lives in the triple
store.

| project | role |
|---|---|
| `Contracts` | ports: `ICatalogSearchEngine`, `ICatalogIndexWriter`, `ICodeListSearchEngine`, `ICodeListIndexWriter`, and the index/hit documents |
| `Business` | `CatalogIndexRebuilder`, `CodeListIndexRebuilder`, the three source interfaces |
| `Elasticsearch` | query builders, mappings, document factories, bulk writer, provisioner, response readers |
| `Data` | the only project touching the data layer; reads through `ISearchIndexProviderService`, maps models to index documents |
| `Api` | the service: endpoints, `ReindexGate`, startup provisioning, API-key filter, health check |

## Done

- Contracts, slimmed to reuse `DataAccess.Abstractions` types rather than duplicate them
- Catalog and code-list engines: search, facets (drill-sideways), bulk indexing, provisioning
- `Data` on `ISearchIndexProviderService` — no entities, no `DbContext`
- `AncestorCodes` computed at index time, replacing a per-search recursive-CTE breadcrumb walk
- `POST /api/index/reindex?reset=true`, `GET /api/index/status`, the three search endpoints
- `ReindexGate` single-flight, with `reset` inside the gate so an index cannot be dropped under a
  running rebuild (a dropped index would be auto-recreated with a dynamic mapping)
- The configured index names are **aliases**. A `reset` pass builds into `{alias}-{stamp}` and moves
  the alias in one atomic request, so a failed or cancelled pass leaves the live index untouched
- `reindex` answers **202** and runs detached from the request, sequenced by one `ReindexOrchestrator`
  shared with the startup pass, so a proxy timeout can no longer cancel a pass mid-flight
- Measured: rebuild 13m49s -> 2m10s; index 570 MB; `statis` matches `statistik` after the ngram fix

## Remaining, in order

### 1. Read cutover

Repoint `GetCatalogSearchCommandHandler` and `GetCatalogSearchCountCommandHandler` off Lucene's
`ICatalogIndexService` onto the new service.

Blocked on the deep-pagination decision: `Paging.ToWindow(1, int.MaxValue)` returns `(0, 10_000)`, so
an unpaginated request truncates silently. Not reachable today because that handler uses Lucene and
`SearchController` clamps to `MaxPageSize = 200`. It becomes live the moment this lands, so decide
here: reject beyond the window, or fix the caller that asks for `int.MaxValue`.

### 2. Write cutover — the dispatcher

Today **26 command handlers** call `ICatalogIndexService.UpdateIndex(...)` inline, synchronously, on
the request thread. Once IndexSearch is a separate process that call is impossible by construction.

Design: handlers publish one thin fact after commit; consumers fan out.

```
Core handler --SaveChanges--> IPublisher.Publish(ResourceChanged(Type, Id, Op))
                                   |
                    +--------------+--------------+
                    v                             v
              AuditTrail consumer          IndexSearch consumer
                                             POST /api/index/notify
                                                  |
                                       coalescing queue (Channel<T>)
                                                  |
                                       IndexUpdateService (BackgroundService)
                                                  |
                        Data (re-read current state) -> Contracts document
                                                  |
                                  ICatalogIndexWriter / ICodeListIndexWriter
                                                  |
                                            Elasticsearch
```

Decisions taken:

- **Payload is `(Type, Id, Operation)` only.** The consumer re-reads current state, so it cannot
  index stale data and the message needs no versioning. `Operation` is required: with type+id alone a
  delete is indistinguishable from a row not yet visible.
- **Coalesce by `(Type, Id)`** in the queue window — correctness (out-of-order collapse) and cost.
- **External versioning in Elasticsearch** (`version_type=external`, monotonic `ModifiedAt`) so a
  stale write is rejected by the server. `ElasticsearchBulkWriter` currently emits no version, so it
  is last-write-wins.
- **No transactional outbox initially.** A lost notification is repaired by the scheduled full
  reindex; a 2m10s rebuild makes that a real answer rather than a hopeful one.
- **HTTP push, broker-agnostic contract.** No messaging package exists in the solution today. Keep
  `ResourceChanged` free of transport types so Service Bus can slot in later untouched. Core must
  treat the call as fire-and-forget: IndexSearch being down must never fail a write.
- **`ResourceChanged` lives in `DataAccess.Abstractions`** — dependency-free, already shared by both
  sides, and already home to `SearchResourceType`.

Two prerequisites this design exposes:

- **The provider has no by-id read.** `ISearchIndexProviderService` is six `Get...InBatches`
  full-scan methods. Without a scoped read, every notification triggers a table scan. Needs by-id
  (or by-id-set) methods.
- **Code lists need a concept-scoped replace.** Catalog is 1 resource = 1 document, but code-list
  documents are per entry and `AncestorCodes` is derived *across* entries, so changing one entry's
  parent invalidates its whole subtree. `ICodeListIndexWriter` has only `WriteAsync` and
  `DeleteAsync(ids)`; it needs a scoped replace (delete-by-query on `conceptId`, then bulk write).

### 3. Scheduled full reindex

`INDEXSEARCH_FULL_REINDEX_INTERVAL_HOURS` is declared in `.env` and implemented nowhere. It is the
drift backstop for stage 2, so it is now load-bearing rather than a nice-to-have.

### 4. Delete Lucene

After stage 2. `Bfs.Iop.Core` still references `Bfs.Iop.Core.Lucene`.

## Open decisions needing an owner

- **Config naming.** `.env` declares `INDEXSEARCH_SECRET`, `INDEXSEARCH_BUILD_ON_STARTUP`,
  `INDEXSEARCH_FULL_REINDEX_INTERVAL_HOURS`; `appsettings.json` expects `#{INDEXSEARCH_API_KEY}#` and
  `#{INDEXSEARCH_REINDEX_ON_STARTUP}#`; `IndexSearchOptions` has no interval property. No workflow or
  container app references any of them. Failure mode is silent: an unsubstituted token makes `ApiKey`
  a literal non-empty string, so every reindex returns 401 while the key looks configured.
- **`BusinessRole` dependency cost.** Consolidating the duplicate enum made `Contracts` reference
  `Infrastructure.Security`, pulling 12 JWT/OpenIdConnect packages into every consumer including the
  Elasticsearch adapter. Fix: move the enum to a dependency-free assembly.
- **Two role-precedence orders.** `SearchCallerFactory.ResolveRole` ranks `SwissDataSteward` second;
  `UserContextService.GetUserBusinessRole` ranks it last. Since `SwissDataSteward` gets no
  authorization clause, a caller holding it plus `LocalDataSteward` is unrestricted under ours and
  agency-scoped under Security's. Ours is deliberate and pinned by a test, but two orders in one
  codebase is a trap.

## Housekeeping

Four directories under `src/IndexSearch/` contain only `bin`/`obj` from projects that no longer
exist — `IndexSearch.Abstractions`, `.ApiClient`, and their test dirs. Untracked leftovers, safe to
delete.

## Verification

1. `dotnet build i14y.slnx -c Debug` — 0 errors, 0 IndexSearch warnings.
2. `dotnet test i14y.slnx -c Debug` — 665 passing, 0 failing, 16 assemblies.
3. Against live Postgres and Elasticsearch, the env-gated fixtures:
   `SearchIndexProviderNavigationTests` (needs only Postgres), then `RebuildPipelineTests` and
   `CodeListSearchTests` after a rebuild.
4. After any mapping change, a full `reindex?reset=true` — `CreateIfMissingAsync` leaves an existing
   index's mapping alone, so a mapping edit is inert until a pass builds a new index and moves the
   alias onto it.
