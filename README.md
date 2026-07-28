# I14Y

I14Y is Switzerland's interoperability platform and metadata catalog.

It is the central Swiss directory for metadata about public-sector datasets, electronic interfaces (APIs), and government services. I14Y makes these assets discoverable, clarifies who is responsible for them, and explains how they can be accessed and reused.

I14Y publishes metadata only, not the underlying operational data. The platform documents key context around each entry, including access conditions (open or restricted), standards alignment, update practices, and quality-related information. This helps organizations understand and compare data offerings before integrating them.

Beyond cataloging, I14Y supports harmonization across organizations by promoting shared concepts, codelists, and structured metadata models. The goal is to make cross-administration data exchange more reliable and to support the once-only principle, so citizens and companies are asked for the same information less often.

The platform primarily serves public authorities across federal, cantonal, and municipal levels, as well as universities and public-sector partners. Publicly released metadata can be consulted and retrieved openly, while organizational workflows and permissions govern publication and maintenance.

I14Y is operated and developed by the Federal Statistical Office (FSO), within the Competence Center for Data Management.

This repository is the monorepo for I14Y backend and frontend components.

## Repository Scope

This monorepo contains:

- Backend .NET services and libraries in `backend/src`
- Frontend Angular applications in `frontend/public-ui` and `frontend/admin-ui`
- Generated admin API TypeScript client assets in `build/ts-client/generated`
- Delivery assets in `docker/Dockerfile.*` and CI workflows in `.github/workflows`

Main backend API projects:

- `backend/src/Core/Bfs.Iop.Core.Api`
- `backend/src/Admin/Bfs.Iop.Admin.Api`
- `backend/src/Partner/Bfs.Iop.Partner.Api`
- `backend/src/Iri/Bfs.Iop.Iri.Api`

## Architecture Overview

```text
backend/  .NET solution, APIs, business/domain/infrastructure libraries, tests
frontend/ Angular applications (public-ui, admin-ui)
build/    Shared generated assets (including TS API client output)
```

The `frontend/*` applications are runtime frontends.

Generated admin API client sources are produced under `build/ts-client/generated` and then copied into each frontend-local API client package.

## API Client Generation Flow

The admin API client flow is:

1. `backend/src/Admin/Bfs.Iop.Admin.Api.ClientGenerator` generates TypeScript client code into `build/ts-client/generated`.
2. Frontend apps copy generated assets with `npm run api:generated`.
3. Frontend-local API client code lives under `frontend/*/api-client`.

No end-to-end npm publish automation for a standalone admin API client package is declared in repository workflows.

## Quick Start

Detailed setup is documented in `GETTING_STARTED.md`.

Minimal local start:

1. Install prerequisites: .NET SDK 10, Node.js 24, npm.
2. Build backend:
   - `dotnet restore backend/i14y.slnx`
   - `dotnet build backend/i14y.slnx -c Release`
3. Run frontends:
   - `cd frontend/public-ui && npm ci && npm run api:generated && npm run start`
   - `cd frontend/admin-ui && npm ci && npm run api:generated && npm run start`

### Run Frontends With Docker (Local Dev)

These Dockerfiles are for local developer usage only. Frontend deployment remains based on Azure Static Web Apps.

1. Build admin-ui image:
   - `docker build -f docker/Dockerfile.admin-ui -t iop-admin-ui:dev .`
   - admin.ch network: `docker build --build-arg NODE_IMAGE=repo.bit.admin.ch:8444/node:24-bookworm-slim -f docker/Dockerfile.admin-ui -t iop-admin-ui:dev .`
2. Run admin-ui on port 4200:
   - `docker run --rm -p 4200:4200 iop-admin-ui:dev`
3. Build public-ui image:
   - `docker build -f docker/Dockerfile.public-ui -t iop-public-ui:dev .`
   - admin.ch network: `docker build --build-arg NODE_IMAGE=repo.bit.admin.ch:8444/node:24-bookworm-slim -f docker/Dockerfile.public-ui -t iop-public-ui:dev .`
4. Run public-ui on port 5022:
   - `docker run --rm -p 5022:5022 iop-public-ui:dev`

### Run Full Local Stack With Docker Compose

You can run the full local stack from root with one compose file:

- Backend APIs: core, admin, partner, iri
- Frontends: admin-ui, public-ui
- Dependencies: postgres, fuseki, keycloak

1. Create a local environment file:
   - `cp .env.compose.example .env`
2. Start everything:
   - `docker compose up --build`
3. Main local URLs:
   - `http://localhost:4200` (admin-ui)
   - `http://localhost:5022` (public-ui)
   - `http://localhost:8000` (core-api)
   - `http://localhost:8001` (admin-api)
   - `http://localhost:8002` (partner-api)
   - `http://localhost:8003` (iri-api)
   - `http://localhost:8080` (keycloak)
   - `http://localhost:3030` (fuseki)

Notes:

- In docker-compose, Visual Studio user secrets are replaced by environment variables (`Section__Key` format).
- Frontend appconfig values are generated at container startup from container environment variables.
- Local auth uses Keycloak over HTTP at `http://keycloak.localtest.me:8080/realms/i14y-local`.

Local login test account (realm `i14y-local`):

- Username: `i14y-user`
- Password: `i14y-password`

Keycloak realm import behavior:

- The file `docker/keycloak/i14y-local-realm.json` is imported at Keycloak startup.
- If the realm already exists, Keycloak may skip updates (`IGNORE_EXISTING`), so changes to users/roles are not applied automatically.
- To force reimport after editing `docker/keycloak/i14y-local-realm.json`:
  - `docker compose rm -sf keycloak`
  - `docker compose up -d keycloak`

Corporate network note (NuGet restore in backend Docker builds):

- Configure proxy variables in `.env` (`HTTP_PROXY`, `HTTPS_PROXY`, `NO_PROXY`).
- Corporate CA certificate injection is optional and only needed when your proxy intercepts TLS.
- Provide certificate paths via `CORPORATE_CA_FILE` and `CORPORATE_CA_CHAIN_FILE` when required.
- Use repository-relative paths with forward slashes, for example:
  - `CORPORATE_CA_FILE=build/certificates/BIT_Proxy_CA_06_C.crt`
  - `CORPORATE_CA_CHAIN_FILE=build/certificates/BIT_Proxy_Root_CA_01.crt`
- If your network does not intercept TLS, keep both certificate variables empty.

