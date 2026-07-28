# Getting Started

This guide covers local development in this monorepo.

## Prerequisites

- Git
- .NET SDK 10.0 (all backend projects target `net10.0`)
- Node.js 24 and npm (CI uses `actions/setup-node` with `node-version: "24"`)
- Docker (optional, for local Elasticsearch and backend image builds)

## Repository Paths

- Backend solution: `backend/i14y.slnx`
- Runtime frontends:
  - `frontend/public-ui`
  - `frontend/admin-ui`
- Generated admin TypeScript API client source: `build/ts-client/generated`

## Backend Quickstart (.NET)

Run from repository root:

```bash
dotnet restore backend/i14y.slnx
dotnet build backend/i14y.slnx -c Release
dotnet test backend/i14y.slnx -c Release
```

Run API projects:

```bash
dotnet run --project backend/src/Core/Bfs.Iop.Core.Api/Bfs.Iop.Core.Api.csproj
dotnet run --project backend/src/Admin/Bfs.Iop.Admin.Api/Bfs.Iop.Admin.Api.csproj
dotnet run --project backend/src/Partner/Bfs.Iop.Partner.Api/Bfs.Iop.Partner.Api.csproj
dotnet run --project backend/src/Iri/Bfs.Iop.Iri.Api/Bfs.Iop.Iri.Api.csproj
```

## Frontend Quickstart (Public UI)

```bash
cd frontend/public-ui
npm ci
npm run api:generated
npm run start
```

Validation commands:

```bash
npm run build
npm run build-prod
npm run lint
npm run lint:fix
npm run prettier
npm run format
npm run audit
```

## Frontend Quickstart (Admin UI)

```bash
cd frontend/admin-ui
npm ci
npm run api:generated
npm run start
```

Optional HTTPS local run:

```bash
npm run start-secure
```

Validation commands:

```bash
npm run build
npm run build-prod
npm run lint
npm run lint:fix
npm run prettier
npm run format
npm run audit
```

## API npm Client Generation Quickstart

Current flow (distinct from runtime frontends):

1. Generate admin API TypeScript client from backend:

```bash
dotnet run --project backend/src/Admin/Bfs.Iop.Admin.Api.ClientGenerator/Bfs.Iop.Admin.Api.ClientGenerator.csproj
```

This writes generated files to:

- `build/ts-client/generated`

2. Copy generated client into each frontend-local API client package:

```bash
cd frontend/public-ui
npm run api:generated
cd ../admin-ui
npm run api:generated
```

The generated client assets are consumed by frontends under `frontend/*/api-client` and are not a runtime frontend by themselves.

## Optional Docker Commands

Run full local stack (APIs + UIs + Keycloak + Postgres + Fuseki):

```bash
cp .env.compose.example .env
docker compose up --build
```

Main URLs:

- `http://localhost:4200` admin-ui
- `http://localhost:5022` public-ui
- `http://localhost:8000` core-api
- `http://localhost:8001` admin-api
- `http://localhost:8002` partner-api
- `http://localhost:8003` iri-api
- `http://keycloak.localtest.me:8080` keycloak

Local Keycloak test account (realm `i14y-local`):

- Username: `i14y-user`
- Password: `i14y-password`

If you edit `docker/keycloak/i14y-local-realm.json` and role/user changes do not apply, force Keycloak reimport:

```bash
docker compose rm -sf keycloak
docker compose up -d keycloak
```

Why: when the realm already exists, Keycloak can start with `IGNORE_EXISTING` and skip importing updates from the JSON file.

Build backend API images:

```bash
docker build -f docker/Dockerfile.core -t i14y-core-api:local .
docker build -f docker/Dockerfile.admin -t i14y-admin-api:local .
docker build -f docker/Dockerfile.partner -t i14y-partner-api:local .
docker build -f docker/Dockerfile.iri -t i14y-iri-api:local .
```

Start local Elasticsearch + Kibana for search development:

```bash
docker compose -f backend/docker-compose.yml up -d
docker compose -f backend/docker-compose.yml down
```
