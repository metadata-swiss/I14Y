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
- Delivery assets at root (`Dockerfile.*`) and CI workflows in `.github/workflows`

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

## Build and Deployment Evidence

Backend container build files:

- `Dockerfile.core`
- `Dockerfile.admin`
- `Dockerfile.partner`
- `Dockerfile.iri`

Repository workflows include:

- `.github/workflows/i14y-backend-dev-deploy-automatic.yml`
- `.github/workflows/i14y-public-ui-dev-deploy.yml`
- `.github/workflows/i14y-admin-ui-dev-deploy.yml`
- `.github/workflows/i14y-frontend-release-deploy.yml`
- `.github/workflows/i14y-prepare-release.yml`

## Governance Files

- `CONTRIBUTING.md`
- `CODE_OF_CONDUCT.md`
- `SECURITY.md`
- `CHANGELOG.md`
- `THIRD-PARTY-LICENSES.md`
- `publiccode.yml`

Component-level docs can exist, but repository governance and publication-oriented docs are maintained at root.
