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
| `POST /api/index/reindex` | read the database and write every document, in place. Minutes; search keeps answering |
| `POST /api/index/reindex?reset=true` | same, but builds a fresh pair of indices with the current mapping and moves both aliases onto them at the end. Search keeps answering from the old pair throughout, so this is how a mapping change ships without downtime |
| `GET /api/index/status` | whether a reindex is running here, and how the last one ended |
| `GET /health` | reports whether both indices exist |

The `/api/index` endpoints require a Keycloak/eIAM bearer token carrying the
`BFS-i14y.interoperabilityservice` role — the same token and the same role the search endpoints already
read to decide what a caller may see. Authorise in swagger and the rebuild is attributable to whoever
asked for it. The startup and scheduled passes call the orchestrator in process and never cross this
boundary at all.

## What this host does not do

- **No migrations.** Core owns the schema. Two services migrating one database is a race.
- **No writes to Postgres at all.** It reads entities to build documents and walks the code list
  hierarchy for breadcrumbs.
- **Authentication is optional on the read endpoints.** Requests without a valid bearer token search
  as anonymous and see only public resources; authenticated requests are filtered using the caller's
  role and agencies.
- **The dataset structure facet needs a triple store.** `AddLinkedDataServices` is registered only when
  `TripleStore:Endpoint` is set; without it a reindex leaves the structure flag untouched and logs an
  error saying so, rather than failing to start over one facet.
