# Getting Started

This guide covers local development in this monorepo.

## Prerequisites

- Git
- .NET SDK 10.0 (all backend projects target `net10.0`)
- Node.js 24 and npm (CI uses `actions/setup-node` with `node-version: "24"`)
- Docker with a running Docker daemon (for the local Compose stack)

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

`npm run start` keeps the checked-in frontend development configuration. This is separate from Docker Compose, whose frontend containers inject their own local runtime URLs.

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

`npm run start` keeps the checked-in frontend development configuration. This is separate from Docker Compose, whose frontend containers inject their own local runtime URLs.

```bash
cd frontend/admin-ui
npm ci
npm run api:generated
npm run start
```

Optional HTTPS local run:

Generate the untracked localhost certificate and key once (requires OpenSSL):

```bash
cd frontend/admin-ui/ssl
openssl req -new -x509 -newkey rsa:2048 -sha256 -nodes -keyout localhost.key -days 3560 -out localhost.crt -config localhost-certificate.cnf
cd ..
npm run start-secure
```

Trust `ssl/localhost.crt` in your operating system if the browser warns about the self-signed certificate.

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

## Local Docker Compose

The root `docker-compose.yml` is for local development only. All published ports are bound to `127.0.0.1`; its demo credentials are public local values and must never be reused outside this stack.

1. Copy the local example configuration:

```bash
cp .env.compose.example .env
```

PowerShell equivalent:

```powershell
Copy-Item .env.compose.example .env
```

2. Validate the rendered Compose configuration, then start the stack:

```bash
docker compose --env-file .env.compose.example config --quiet
docker compose up --build -d
docker compose ps
```

The first Core API startup waits for PostgreSQL, applies database migrations, and idempotently inserts the bundled sample data. Follow startup progress with `docker compose logs -f`; the initial image build can take several minutes.

Main local URLs:

- `http://localhost:4200` — Admin UI
- `http://localhost:5022` — Public UI
- `http://localhost:5050` — Core API
- `http://localhost:5010` — Admin API
- `http://localhost:5282` — Partner API
- `http://localhost:5288` — IRI API
- `http://keycloak.localhost:8080` — Keycloak
- `http://localhost:3030` — Fuseki

The bundled Keycloak realm is `i14y-local`. Its public local-only test account has the roles needed to edit sample content:

- Username: `i14y-user`
- Password: `i14y-password`

Keycloak's administrative login also uses the local-only `admin` / `admin` values from `.env`.

### Manual local smoke check

On a clean machine with a running Docker daemon:

1. Run the validation and startup commands above, then confirm `docker compose ps` shows the services running and displays only `127.0.0.1` port bindings.
2. Open the Admin UI, choose the login button, and complete the Keycloak login with `i14y-user`.
3. Open a seeded catalog entry, confirm its sample metadata is visible, make one permitted metadata edit, and save it.
4. Open the Public UI and confirm the seeded data can be read.
5. Record the result in the change or pull request.

### Resetting local data and Keycloak

The following command permanently removes this local stack's PostgreSQL and Fuseki volumes before the next startup recreates and reseeds them:

```bash
docker compose down -v
```

When only the Keycloak realm JSON changed, force a fresh realm import:

```bash
docker compose rm -sf keycloak
docker compose up -d keycloak
```

### Corporate build settings

No proxy, corporate CA, or private registry is enabled by default. If a corporate network requires one during image builds, explicitly uncomment and set the `I14Y_BUILD_*` values in `.env`; do not add them to the copy-and-run defaults.

### Integrations not included locally

The local stack intentionally does not provide EIAM federation or its SOAP service, geocat, opendata.swiss, LINDAS, dashboards, analytics, or external object storage. Their local Compose endpoints are disabled placeholders, so features depending on them are unavailable. The local login flow uses the bundled Keycloak realm instead.

## Build individual backend images (optional)

```bash
docker build -f docker/Dockerfile.core -t i14y-core-api:local .
docker build -f docker/Dockerfile.admin -t i14y-admin-api:local .
docker build -f docker/Dockerfile.partner -t i14y-partner-api:local .
docker build -f docker/Dockerfile.iri -t i14y-iri-api:local .
```
