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

- Backend .NET services and libraries in `src/i14y`
- Frontend Angular applications in `src/ui/public-ui` and `src/ui/admin-ui`
- API npm client generation project in `src/i14y/bfs-iop-admin-ui`
- Delivery assets at root (`Dockerfile.*`) and CI workflows in `.github/workflows`

Main backend API projects:

- `src/i14y/Bfs.Iop.Core.Api`
- `src/i14y/Bfs.Iop.Admin.Api`
- `src/i14y/Bfs.Iop.Partner.Api`
- `src/i14y/Bfs.Iop.Iri.Api`

## Architecture Overview

```text
src/
	i14y/  .NET solution, APIs, business/domain/infrastructure libraries, tests
	ui/    Angular applications (public-ui, admin-ui)
build/   Shared build assets
```

The `src/ui/*` applications are runtime frontends.

The `src/i14y/bfs-iop-admin-ui` project is different: it packages the generated admin web API TypeScript client as an npm library.

## API Client Generation Flow

The admin API client flow is evidence-based from repository sources:

1. `src/i14y/Bfs.Iop.Admin.Api.ClientGenerator` generates TypeScript client code into `src/i14y/bfs-iop-admin-ui/projects/bfs-sis/bfs-iop-admin-web-api-client/src/lib/generated`.
2. `src/i14y/bfs-iop-admin-ui` builds the Angular library with ng-packagr.
3. Frontend apps (`src/ui/public-ui`, `src/ui/admin-ui`) consume `@I14Y-ch/bfs-iop-admin-web-api-client`.

The package is configured for GitHub Packages (`publishConfig.registry = https://npm.pkg.github.com`). No end-to-end npm publish automation for that package is declared in repository workflows.

## Quick Start

Detailed setup is documented in `GETTING_STARTED.md`.

Minimal local start:

1. Install prerequisites: .NET SDK 10, Node.js 24, npm.
2. Build backend:
   - `dotnet restore src/i14y/i14y.slnx`
   - `dotnet build src/i14y/i14y.slnx -c Release`
3. Run frontends:
   - `cd src/ui/public-ui && npm ci && npm run start`
   - `cd src/ui/admin-ui && npm ci && npm run start`

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
- `.github/workflows/prepare-release.yml`

## Governance Files

- `CONTRIBUTING.md`
- `CODE_OF_CONDUCT.md`
- `SECURITY.md`
- `CHANGELOG.md`
- `THIRD-PARTY-LICENSES.md`
- `publiccode.yml`

Component-level docs can exist, but repository governance and publication-oriented docs are maintained at root.
