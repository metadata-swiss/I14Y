# Bfs.Iop.IndexSearch.Api

Reads the catalog and code lists from Postgres, indexes them into Elasticsearch, and serves search.

## Running locally

Needs the local stack:

```bash
docker compose up -d postgres elasticsearch
```

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
| `POST /api/index/reindex?reset=true` | same, but drop both indices and reapply the mapping first. Search returns nothing until it finishes |
| `GET /api/index/status` | whether a reindex is running here, and how the last one ended |
| `GET /health` | reports whether both indices exist |

The `/api/index` endpoints require the `X-Index-Api-Key` header, matching `IndexSearch:ApiKey`. In
Development an unset key leaves them open so swagger works; **anywhere else an unset key closes them**,
because a missing secret must not mean no secret required.

## What this host does not do

- **No migrations.** Core owns the schema. Two services migrating one database is a race.
- **No writes to Postgres at all.** It reads entities to build documents and walks the code list
  hierarchy for breadcrumbs.
- **No authentication on the read endpoints yet**, so every search runs as an anonymous caller and sees
  public resources only. The authorization clause is fail-closed, so that is safe but not yet correct —
  wiring the real claims is outstanding work.
- **The dataset structure facet needs a triple store.** `AddLinkedDataServices` is registered only when
  `TripleStore:Endpoint` is set; without it a reindex leaves the structure flag untouched and logs an
  error saying so, rather than failing to start over one facet.
