# Bfs.Iop.IndexSearch.Api

Reads the catalog and code lists from Postgres, indexes them into Elasticsearch, and serves search.

## Running locally

Needs the local stack:

```bash
docker compose up -d postgres elasticsearch
```

from the repository root, where the compose file lives. Kibana is optional and useful for looking at
the indices: add `kibana` to that command and open <http://localhost:5601>.

Database credentials come from user secrets, in the same shape Core reads them — this project has its
own `UserSecretsId`, so set them once:

```bash
dotnet user-secrets set "postgres:client:hostname" "localhost"
```

The keys are `postgres:client:hostname`, `:port`, `:username`, `:password`, `:database`, matching
`postgresCredentialsSectionKey` in `appsettings.Development.json`. Values are the ones in the
repository-root `.env` that the postgres container starts with.

Then `dotnet run`, and the swagger page is at <http://localhost:5055/swagger>.

## Endpoints

| | |
|---|---|
| `GET /api/search/catalog` | catalog search |
| `GET /api/search/catalog/facets` | drill-sideways facet counts |
| `GET /api/search/codelists/{conceptId}` | entries of one code list, `includePaths` for breadcrumbs |
| `POST /api/index/reindex` | builds a fresh pair of indices with the current mapping and atomically moves both aliases onto them; search keeps answering from the old pair until the swap |
| `GET /api/index/status` | whether a reindex is running here, and how the last one ended |
| `GET /health` | reports whether both indices exist |

The `/api/index` endpoints require a Keycloak/eIAM bearer token carrying the
`BFS-i14y.interoperabilityservice` role — the same token and the same role the search endpoints already
read to decide what a caller may see. Authorise in swagger and the rebuild is attributable to whoever
asked for it. The startup and scheduled passes call the orchestrator in process and never cross this
boundary at all.

## What this host does not do

- **It runs as exactly one instance, and that is a correctness constraint rather than a capacity
  choice.** `ReindexGate` is a `SemaphoreSlim` and `PendingIndexWrites` a dictionary — both live in the
  process and neither is shared. A second instance holds its own of each, so two rebuilds can run at
  once, each swapping the aliases and deleting the generation the other just published; and a write
  routed to the idle instance is never journalled, so the other instance's swap discards it with
  nothing logged.

  A rolling update is the same situation for a few seconds, which is why the deployment in
  `iop-infra-iac` under `stack/07-aks-platform/workloads/indexsearch` pins `replicas: 1` and
  `strategy: Recreate`. **Do not raise the replica count or switch to `RollingUpdate`** without first
  replacing the gate with a distributed lease and the journal with shared, durable storage. Nothing
  here enforces it: the service cannot tell how many copies of itself are running, so the constraint
  is only as good as the manifest.

- **Incremental updates are best effort, not guaranteed delivery.** Core queues each change in an
  in-process `ChannelMessageQueue` and `SearchIndexDispatcherService` posts it here, retrying with a
  doubling delay capped at a minute — about four minutes over ten attempts, which covers a rollout of
  this service and a brief Elasticsearch pause. After that it logs that it gave up and moves on.

  A notification is therefore lost if Core restarts with messages still queued, or if this service is
  unreachable for longer than that. **The scheduled rebuild is the repair**, and it is what makes the
  trade-off acceptable: a full pass takes minutes, so "wrong until tonight" is a bounded, known state
  rather than an open-ended one.

  A transactional outbox — written in the same transaction as the change and removed only once
  delivered — is the proper fix and was deliberately left out of the first version. It is worth
  building when the nightly repair stops being good enough, not before.

- **No migrations.** Core owns the schema. Two services migrating one database is a race.
- **No writes to Postgres at all.** It reads entities to build documents and walks the code list
  hierarchy for breadcrumbs.
- **Authentication is optional on the read endpoints.** Requests without a valid bearer token search
  as anonymous and see only public resources; authenticated requests are filtered using the caller's
  role and agencies.
- **The dataset structure facet needs a triple store.** `AddLinkedDataServices` is registered only when
  `TripleStore:Endpoint` is set; without it a reindex leaves the structure flag untouched and logs an
  error saying so, rather than failing to start over one facet.
