# Getting Started

This guide covers local development in this monorepo.

## Prerequisites

- Git
- Docker with the Compose v2 plugin and a running Docker daemon (for the local Compose stack)
- .NET SDK 10.0 and Node.js 24/npm only when running the backend or frontend directly on the host. Compose builds and runs them inside containers.

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

From the repository root, start the stack:

```bash
docker compose up --build
```

This command also works in PowerShell. No `.env` file is required for the defaults on a network with direct access to the image and package registries. Keep ports 4200, 5022, 5050, 5010, 5282, 5288, 5055, 8080, 3030, 5432, 9200 and 10000 free.

If you need custom local values or corporate certificates, copy the example **once**, edit `.env`, and then run the command above. Preserve an existing `.env`:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.compose.example .env }
```

On macOS/Linux:

```bash
test -f .env || cp .env.compose.example .env
```

Startup logs appear directly in the terminal. Compose uses the configured health checks to start dependent services in order: PostgreSQL, Fuseki, Keycloak and AuditTrail, then Core, IndexSearch and the interfaces. Core applies migrations and idempotently inserts the sample data before accepting HTTP requests. IndexSearch waits for that initialization before filling an empty search index. The first build can take several minutes. AuditTrail is internal to the stack and stores its Git history in a named volume.

In a second terminal, use `docker compose ps -a` to see stopped and unhealthy services. Container names are scoped to the Compose project; use service names in commands such as `docker compose logs core-api`.

Main local URLs:

- `http://localhost:4200` — Admin UI
- `http://localhost:5022` — Public UI
- `http://localhost:5050` — Core API
- `http://localhost:5010` — Admin API
- `http://localhost:5282` — Partner API
- `http://localhost:5288` — IRI API
- `http://keycloak.localhost:8080` — Keycloak
- `http://localhost:3030` — Fuseki

The two interfaces also accept `http://127.0.0.1:4200` and `http://127.0.0.1:5022`; both loopback hostnames are allowed in Keycloak and the Admin API's CORS policy.

The bundled Keycloak realm is `i14y-local`. Its public local-only test account has the roles needed to edit sample content:

- Username: `i14y-user`
- Password: `i14y-password`

Keycloak's administrative login also uses the local-only `admin` / `admin` values from `.env`.

### Optional startup validation and manual smoke check

For an automated startup check, use this alternative command:

```bash
docker compose up --build --wait --wait-timeout 300
```

`--wait` starts the stack in the background and waits for its health checks to pass. `--wait-timeout 300` reports failure if the services are not ready within five minutes. These options are only needed for this validation; normal startup already respects the dependencies and health checks configured in Compose.

On a clean machine with a running Docker daemon, use a second terminal for the status command if the startup logs are still attached to the first:

1. Run the startup command above, then confirm `docker compose ps -a` shows all required services healthy and displays only `127.0.0.1` port bindings.
2. Open the Admin UI, choose the login button, and complete the Keycloak login with `i14y-user`.
3. Open a seeded catalog entry, confirm its sample metadata is visible, make one permitted metadata edit, and save it.
4. Open the Public UI and confirm the seeded data can be read.
5. Record the result in the change or pull request.

### Resetting local data and Keycloak

To repair existing containers that have lost their network attachments, recreate them while preserving the named data volumes:

```bash
docker compose up --force-recreate
```

The following command permanently removes this local stack's named data volumes (PostgreSQL, Fuseki, Elasticsearch, Azurite and AuditTrail) before the next startup recreates and reseeds them:

```bash
docker compose down -v
```

When only the Keycloak realm JSON changed, force a fresh realm import:

```bash
docker compose up -d --force-recreate keycloak
```

### Corporate build settings

No proxy, corporate CA, or private registry is enabled by default. On a network that intercepts HTTPS, obtain its root and intermediate CA certificates in PEM/Base-64 format and copy them into `build/certificates/`. Set their paths in `.env`, relative to the repository root:

```dotenv
I14Y_CORPORATE_CA_FILE=build/certificates/corporate-root.crt
I14Y_CORPORATE_CA_CHAIN_FILE=build/certificates/corporate-intermediate.crt
```

`I14Y_CORPORATE_CA_FILE` is the root CA; `I14Y_CORPORATE_CA_CHAIN_FILE` is the intermediate CA, if required. Use your actual filenames. Files outside the repository are not part of Docker's build context. Keep these local files and `.env` out of commits. For the BIT certificates, use `BIT_Proxy_Root_CA_01.crt` and `BIT_Proxy_CA_06_C.crt`, respectively.

If required, also set the proxy `I14Y_BUILD_*` values or `I14Y_FRONTEND_NODE_IMAGE` for an approved Node image. The CA settings configure the backend image builds; they do not install certificates into third-party images such as Kibana. Do not disable HTTPS certificate verification.

### Optional Kibana

Kibana is a diagnostic tool and is excluded from the default stack. Elasticsearch and IndexSearch remain enabled because the application needs them. Start Kibana explicitly when needed:

```bash
docker compose --profile tools up -d kibana
```

Open `http://localhost:5601`. On corporate networks its external Elastic registry and telemetry connections can require additional proxy CA configuration. An already-running Kibana from an older Compose configuration can be stopped with `docker compose stop kibana`.

### Object storage

The Core API keeps media, dataset structure graphs, and filter configurations in Azure Blob Storage.
Locally that is the Azurite emulator, which Compose provides:

```bash
docker compose up -d azurite
```

It publishes the blob endpoint on `127.0.0.1:10000`, which is what
`backend/src/Core/Bfs.Iop.Core.Api/appsettings.Development.json` already points at, so an API run from
Visual Studio or `dotnet run` needs no further configuration. The `core-api` Compose service overrides
the connection string to reach the same emulator at `azurite:10000` instead, because `127.0.0.1` inside
that container is the container itself.

Containers are created on first write, so nothing needs seeding. Without Azurite running, any request
that touches storage fails after the Azure SDK exhausts its six retries.

### IndexSearch is an internal service

`indexsearch-api` answers without authentication, by design: Core forwards the caller's token, and a
public catalogue search has no token to forward. The catalogue routes are safe on their own, because
the caller's role becomes a filter on `publicationLevel` and an anonymous caller sees only public
resources. **The code list routes are not.** A code list document carries no publication level, so the
only check that a caller may read a concept's entries lives in Core, and reaching the search service
directly bypasses it.

Compose therefore publishes it on `127.0.0.1:5055`, and any deployment must give it internal-only
ingress. Making it publicly reachable would first require stamping each concept's publication level
onto its code list documents and applying the catalogue's authorization clause to the code list query.

### Integrations not included locally

The local stack intentionally does not provide EIAM federation or its SOAP service, geocat, opendata.swiss, LINDAS, dashboards, or analytics. Their local Compose endpoints are disabled placeholders, so features depending on them are unavailable. The local login flow uses the bundled Keycloak realm instead.

## Build individual backend images (optional)

```bash
docker build -f docker/Dockerfile.core -t i14y-core-api:local .
docker build -f docker/Dockerfile.admin -t i14y-admin-api:local .
docker build -f docker/Dockerfile.partner -t i14y-partner-api:local .
docker build -f docker/Dockerfile.iri -t i14y-iri-api:local .
```

Start local Elasticsearch + Kibana for search development:

```bash
docker compose up -d elasticsearch kibana
docker compose stop elasticsearch kibana
```

Elasticsearch answers on <http://localhost:9200> and Kibana on <http://localhost:5601>. The
`[Explicit]` integration fixtures under `src/IndexSearch` need this running; without it they are
skipped rather than failed.
