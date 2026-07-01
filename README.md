# I14Y

I14Y is Switzerland's interoperability platform and metadata catalog.

It is the central Swiss directory for metadata about public-sector datasets, electronic interfaces (APIs), and government services. I14Y makes these assets discoverable, clarifies who is responsible for them, and explains how they can be accessed and reused.

I14Y publishes metadata only, not the underlying operational data. The platform documents key context around each entry, including access conditions (open or restricted), standards alignment, update practices, and quality-related information. This helps organizations understand and compare data offerings before integrating them.

Beyond cataloging, I14Y supports harmonization across organizations by promoting shared concepts, codelists, and structured metadata models. The goal is to make cross-administration data exchange more reliable and to support the once-only principle, so citizens and companies are asked for the same information less often.

The platform primarily serves public authorities across federal, cantonal, and municipal levels, as well as universities and public-sector partners. Publicly released metadata can be consulted and retrieved openly, while organizational workflows and permissions govern publication and maintenance.

I14Y is operated and developed by the Federal Statistical Office (FSO), within the Competence Center for Data Management.

This repository is the monorepo that contains the backend APIs and frontend applications used to run I14Y.

## What This Repository Contains

This monorepo groups the platform components in one place:

- Backend (.NET): APIs and domain/business/infrastructure projects under `src/i14y`
- Frontend (Angular): public and admin web applications under `src/ui`
- Delivery assets: Dockerfiles and GitHub Actions workflows at repository root and `.github/workflows`

Main backend API projects:

- `src/i14y/Bfs.Iop.Core.Api`
- `src/i14y/Bfs.Iop.Admin.Api`
- `src/i14y/Bfs.Iop.Partner.Api`
- `src/i14y/Bfs.Iop.Iri.Api`

Main frontend applications:

- `src/ui/public-ui`
- `src/ui/admin-ui`

## Repository Map

```text
src/
	i14y/        # .NET solution and projects (core/admin/partner/iri + libraries + tests)
	ui/          # Angular applications (public-ui, admin-ui)
build/         # shared build assets
.github/       # CI/CD workflows and Copilot prompts
Dockerfile.*   # container builds for backend APIs
```

## Getting Started

Detailed setup is in `GETTING_STARTED.md`.

Quick start:

1. Install prerequisites
   - .NET SDK 10
   - Node.js 24 + npm
2. Backend
   - `dotnet restore src/i14y/i14y.slnx`
   - `dotnet build src/i14y/i14y.slnx -c Release`
3. Frontend
   - `cd src/ui/public-ui && npm ci && npm run start`
   - `cd src/ui/admin-ui && npm ci && npm run start`

## Build and Deployment Overview

- Backend images are built from:
  - `Dockerfile.core`
  - `Dockerfile.admin`
  - `Dockerfile.partner`
  - `Dockerfile.iri`
- CI/CD workflows are in `.github/workflows`, including:
  - `i14y-backend-dev-deploy-automatic.yml`
  - `i14y-public-ui-dev-deploy.yml`
  - `i14y-admin-ui-dev-deploy.yml`
  - `prepare-release.yml`

## Documentation and Governance

Documentation for this repository is maintained at the root level.

- Contribution guide: `CONTRIBUTING.md`
- Code of conduct: `CODE_OF_CONDUCT.md`
- Security policy: `SECURITY.md`
- Changelog: `CHANGELOG.md`
- Third-party licenses: `THIRD-PARTY-LICENSES.md`
- Public software metadata: `publiccode.yml`

Component-level docs may exist for local context, but governance and publication information is maintained in the root files.
