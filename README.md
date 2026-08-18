# I14Y

<p align="center">
   <img src="frontend/public-ui/src/assets/images/NaDB-Interoper_l14Y.png" alt="I14Y logo" width="260" />
</p>

I14Y is Switzerland's interoperability platform and metadata catalog for the public sector.

This repository is the monorepo for I14Y backend and frontend components.

I14Y is a central directory of metadata for datasets, electronic interfaces (APIs), and public services. It makes assets easier to find, clarifies ownership, and explains access and reuse conditions. The platform publishes metadata, not operational source data. It also supports semantic harmonization through shared concepts and codelists, so data can be reused across authorities and systems.

Federal, cantonal, and municipal authorities are the main users, alongside public-sector partners. Publicly released metadata can be queried through the web interface and APIs, while publication and maintenance follow role-based governance workflows.

> [!IMPORTANT]
> I14Y catalogs metadata and interoperability context. It is not a data lake for business records.

## Repository Map

- backend: .NET solution and projects (APIs, domain/business/infrastructure libraries, tests)
- frontend/public-ui: Angular application for public-facing scenarios
- frontend/admin-ui: Angular application for administration workflows
- build/ts-client/generated: generated TypeScript API client artifacts
- docker: Dockerfiles and local runtime helpers
- .github/workflows: CI/CD workflows for backend and frontend delivery

Main backend API projects:

- backend/src/Core/Bfs.Iop.Core.Api
- backend/src/Admin/Bfs.Iop.Admin.Api
- backend/src/Partner/Bfs.Iop.Partner.Api
- backend/src/Iri/Bfs.Iop.Iri.Api

## Technology Stack

- Backend: .NET 10, multi-project solution in backend/i14y.slnx
- Frontend: Angular 21 applications (Node.js 24 in workflows)
- Containerization: Dockerfiles for each API and both frontends
- CI/CD: GitHub Actions workflows in .github/workflows
- Local integration stack: docker-compose with Postgres, Fuseki, Keycloak, APIs, and UIs

## Architecture

```text
backend/   APIs + shared libraries + tests
frontend/  Runtime Angular apps (public-ui, admin-ui)
build/     Shared generated assets (including TS client output)
docker/    Container build/runtime definitions
```

Monorepo boundaries:

- Runtime frontends are only under frontend/\*.
- Generated API client output under build/ts-client/generated is a build artifact, not a frontend app.

How to add new modules:

- New backend module: add project(s) under backend/src/<domain>, reference from backend/i14y.slnx, and align with existing API/library/test split.
- New frontend module: add under frontend/<app>, keep app-local api-client usage pattern, and integrate with CI workflow conventions.

## API Client Generation Flow

1. Run backend/src/Admin/Bfs.Iop.Admin.Api.ClientGenerator to generate TypeScript client sources.
2. Generated output is written to build/ts-client/generated.
3. Each frontend imports generated files with npm run api:generated into frontend/\*/api-client/lib/generated.

> [!NOTE]
> No standalone npm package publication is configured in this repository for the generated admin client.

## Getting Started

See GETTING_STARTED.md for full local setup. The commands below are the quickest local path.

Prerequisites:

- .NET SDK 10.0
- Node.js 24 and npm
- Docker (optional, for containerized local runs)

Backend build and test:

```bash
dotnet restore backend/i14y.slnx
dotnet build backend/i14y.slnx -c Release
dotnet test backend/i14y.slnx -c Release
```

Run backend APIs:

```bash
dotnet run --project backend/src/Core/Bfs.Iop.Core.Api/Bfs.Iop.Core.Api.csproj
dotnet run --project backend/src/Admin/Bfs.Iop.Admin.Api/Bfs.Iop.Admin.Api.csproj
dotnet run --project backend/src/Partner/Bfs.Iop.Partner.Api/Bfs.Iop.Partner.Api.csproj
dotnet run --project backend/src/Iri/Bfs.Iop.Iri.Api/Bfs.Iop.Iri.Api.csproj
```

Public UI:

```bash
cd frontend/public-ui
npm ci
npm run api:generated
npm run start
```

Admin UI:

```bash
cd frontend/admin-ui
npm ci
npm run api:generated
npm run start
```

Optional HTTPS run for admin UI:

```bash
cd frontend/admin-ui
npm run start-secure
```

Local Docker for frontends:

1. Build admin UI image:
   - `docker build -f docker/Dockerfile.admin-ui -t iop-admin-ui:dev .`
   - admin.ch network: `docker build --build-arg NODE_IMAGE=repo.bit.admin.ch:8444/node:24-bookworm-slim -f docker/Dockerfile.admin-ui -t iop-admin-ui:dev .`
2. Run admin UI on port 4200:
   - `docker run --rm -p 4200:4200 iop-admin-ui:dev`
3. Build public UI image:
   - `docker build -f docker/Dockerfile.public-ui -t iop-public-ui:dev .`
   - admin.ch network: `docker build --build-arg NODE_IMAGE=repo.bit.admin.ch:8444/node:24-bookworm-slim -f docker/Dockerfile.public-ui -t iop-public-ui:dev .`
4. Run public UI on port 5022:
   - `docker run --rm -p 5022:5022 iop-public-ui:dev`

Local Docker Compose full stack:

```bash
cp .env.compose.example .env
docker compose up --build
```

Main local URLs:

- http://localhost:4200 (admin-ui)
- http://localhost:5022 (public-ui)
- http://localhost:8000 (core-api)
- http://localhost:8001 (admin-api)
- http://localhost:8002 (partner-api)
- http://localhost:8003 (iri-api)
- http://keycloak.localtest.me:8080 (keycloak)
- http://localhost:3030 (fuseki)

Local Keycloak test account (realm i14y-local):

- Username: i14y-user
- Password: i14y-password

If Keycloak realm updates are not applied after editing docker/keycloak/i14y-local-realm.json, force reimport:

```bash
docker compose rm -sf keycloak
docker compose up -d keycloak
```

Corporate network note for backend Docker restore:

- Set HTTP_PROXY, HTTPS_PROXY, and NO_PROXY in .env.
- If TLS interception is in place, set optional certificate paths:
  - `CORPORATE_CA_FILE=build/certificates/BIT_Proxy_CA_06_C.crt`
  - `CORPORATE_CA_CHAIN_FILE=build/certificates/BIT_Proxy_Root_CA_01.crt`
- If your network does not intercept TLS, keep those certificate variables empty.

## Build and Deployment Evidence

Backend image build and deployment:

- docker/Dockerfile.core
- docker/Dockerfile.admin
- docker/Dockerfile.partner
- docker/Dockerfile.iri
- .github/workflows/i14y-backend-dev-deploy-automatic.yml
- .github/workflows/i14y-backend-deploy.yml

Frontend build and deployment:

- docker/Dockerfile.public-ui
- docker/Dockerfile.admin-ui
- .github/workflows/i14y-public-ui-dev-deploy.yml
- .github/workflows/i14y-admin-ui-dev-deploy.yml
- .github/workflows/i14y-frontend-release-deploy.yml

Local full-stack compose:

- docker-compose.yml defines postgres, fuseki, keycloak, core-api, admin-api, partner-api, iri-api, admin-ui, and public-ui.

## Quality and Troubleshooting

Frontend validation commands (both apps):

- npm run build
- npm run build-prod
- npm run lint
- npm run lint:fix
- npm run prettier
- npm run format
- npm run audit

Troubleshooting tips:

- If generated API client imports break, rerun generator then npm run api:generated in each frontend.
- If Keycloak realm changes are not applied in Docker, follow the Keycloak reimport steps in the Local Docker Compose full stack section above.
- If backend Docker restore fails in a corporate network, apply the proxy and certificate settings from the Corporate network note in Getting Started.
- If frontend runtime configuration looks wrong, verify token replacement from src/assets/config/appconfig.token.json to appconfig.json during build/startup.
