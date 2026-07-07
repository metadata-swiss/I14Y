# BFS IOP Admin UI (I14Y)

I14Y is Switzerland's interoperability platform and metadata catalog for datasets, electronic interfaces, and public services.

This repository contains the Angular administration frontend for I14Y. It is used to create, update, and maintain catalog metadata and core domain entities, including datasets, data services, public services, concepts, and mapping tables.

## Functional Areas

Main application areas are organized under `src/app`:

- `catalog`: entry catalog and filtered views (`all`, `datasets`, `publicservices`, `dataservices`, `concepts`, `mappingtables`)
- `concepts`: concept management (lazy loaded)
- `datasets`: dataset management (lazy loaded)
- `dataservices`: data service management (lazy loaded)
- `mappingtables`: mapping table management (lazy loaded)
- `publicservices`: public service management (lazy loaded)
- `auth`: authentication guards, OIDC callbacks, and API auth interceptor

## Technology Stack

- Angular 21 (`@angular/core`, `@angular/router`, Angular CLI/build tooling)
- Angular Material / CDK (`@angular/material`, `@angular/cdk`)
- Oblique UI framework (`@oblique/oblique`)
- Internationalization (`@ngx-translate/core`)
- Authentication client (`oidc-client-ts`)
- Logging (`ngx-logger`)
- Testing with Jest (`jest`, `@angular-builders/jest`, `jest-preset-angular`)
- Code quality tooling (`eslint`, `@angular-eslint/*`, `prettier`)

## Setup

### Prerequisites

- Node.js and npm installed

### Install Dependencies

```bash
npm install
```

## Run and Build

Use the repository scripts from `package.json`:

```bash
# start dev server (HTTP)
npm run start

# start dev server with local SSL certificate
npm run start-secure

# build
npm run build

# production build
npm run build-prod
```

Alternative local start (for custom host/port and polling file watch):

```bash
npm ci && npx ng serve --host 0.0.0.0 --port 5022 --poll=2000
```

Use `--poll` only when file changes are not detected reliably (for example network drives, containers, or WSL-mounted folders). If file watching works normally, remove `--poll` to reduce CPU usage.

## Testing and Code Quality

```bash
# unit tests (Jest)
npm run test

# lint checks
npm run lint

# lint autofix + prettier formatting
npm run format
```

Notes:

- Jest configuration is in `tests/jest.config.js` and currently collects coverage into `coverage/sonarQube`.
- ESLint rules are defined in `.eslintrc.json`.
- Prettier configuration is in `.prettierrc`.

## Architecture

### Folder Conventions

- `src/app`: feature and core application code
- `src/app/<feature>`: feature modules and UI for each domain area
- `src/app/<feature>/services`: feature-specific services
- `src/app/<feature>/description` (and similar subfolders): feature-specific subviews/edit sections
- `src/app/shared`: reusable components, helpers, pipes, and shared utilities
- `src/app/services`: cross-cutting services used across features
- `src/assets`: static assets, i18n files, and runtime configuration files
- `src/environments`: Angular build-time environment toggles

### Routing and Feature Loading

- Top-level routes are defined in `src/app/app-routing.module.ts`.
- `datasets`, `dataservices`, `publicservices`, `mappingtables`, and `concepts` are lazy-loaded by route.
- Auth guards protect most application routes.

### Where to Add New Features

1. Add a new folder under `src/app/<new-feature>`.
2. Add a feature module (for route-based areas, follow lazy-loaded module pattern).
3. Keep feature-specific API/state logic in `src/app/<new-feature>/services`.
4. Register routes in `src/app/app-routing.module.ts` or in the feature module routing.
5. Reuse shared utilities/components from `src/app/shared` when possible.

## Configuration

Configuration is split between build-time and runtime files:

- Build-time environment flags:
  - `src/environments/environment.ts`
  - `src/environments/environment.prod.ts`
- Runtime application config loaded from:
  - `src/assets/config/appconfig.json`

The runtime config contains environment-specific endpoints and auth settings (for example API base URL and OIDC authority/client values). A tokenized template exists in `src/assets/config/appconfig.token.json` for placeholder-based replacement.

Do not commit secrets to `src/assets/config`.

## Troubleshooting

- `npm run start-secure` fails with SSL errors:
  - Ensure `ssl/localhost.crt` and `ssl/localhost.key` exist and are valid.
- Hot reload does not detect file changes:
  - Start with polling: `npx ng serve --poll=2000`.
  - If CPU usage is high, try increasing interval (for example `--poll=3000`) or remove polling when not needed.
- App starts but backend/auth calls fail:
  - Verify endpoint and OIDC values in `src/assets/config/appconfig.json`.
- Unexpected route redirects:
  - Check guard behavior and route definitions in `src/app/app-routing.module.ts`.
- Test command keeps watching:
  - Jest is configured with `watch: true` in Angular test builder options; this is expected for local development runs.
