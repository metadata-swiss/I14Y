# Getting Started

This guide helps you run I14Y from this monorepo.

## Prerequisites

- Git
- Docker (optional, for container builds)
- .NET SDK 10
- Node.js 24 and npm

## Clone

```bash
git clone https://github.com/I14Y-ch/I14Y.git
cd I14Y
```

## Backend (.NET)

The backend projects are grouped in `src/i14y` and listed in `src/i14y/i14y.slnx`.

### Restore and Build

```bash
dotnet restore src/i14y/i14y.slnx
dotnet build src/i14y/i14y.slnx -c Release
```

### Run APIs locally

Examples:

```bash
dotnet run --project src/i14y/Bfs.Iop.Core.Api/Bfs.Iop.Core.Api.csproj
dotnet run --project src/i14y/Bfs.Iop.Admin.Api/Bfs.Iop.Admin.Api.csproj
dotnet run --project src/i14y/Bfs.Iop.Partner.Api/Bfs.Iop.Partner.Api.csproj
dotnet run --project src/i14y/Bfs.Iop.Iri.Api/Bfs.Iop.Iri.Api.csproj
```

### Run tests

```bash
dotnet test src/i14y/i14y.slnx -c Release
```

## Frontend (Angular)

Two UI applications are provided in `src/ui`.

### Public UI

```bash
cd src/ui/public-ui
npm ci
npm run start
```

Useful commands:

```bash
npm run build
npm run build-prod
npm run lint
npm run lint:fix
npm run prettier
npm run audit
npm run format
```

### Admin UI

```bash
cd src/ui/admin-ui
npm ci
npm run start
```

Useful commands:

```bash
npm run start-secure
npm run build
npm run build-prod
npm run lint
npm run lint:fix
npm run prettier
npm run audit
npm run format
```

## Docker builds (backend)

Backend API images can be built with:

```bash
docker build -f Dockerfile.core -t i14y-core-api:local .
docker build -f Dockerfile.admin -t i14y-admin-api:local .
docker build -f Dockerfile.partner -t i14y-partner-api:local .
docker build -f Dockerfile.iri -t i14y-iri-api:local .
```

## Notes

- This guide covers the standard onboarding path for this repository.
- Environment-specific values should be provided via local config files and deployment variables, not committed as secrets.
