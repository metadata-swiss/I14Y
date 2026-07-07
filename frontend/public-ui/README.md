# BFS IOP Public UI

I14Y is Switzerland's national interoperability platform and metadata catalog.
This repository contains the Angular public frontend used to browse and explore the I14Y metadata catalog.

## Business Context

I14Y is the central Swiss directory for public-sector data, electronic interfaces (APIs), and electronic public services.
The platform publishes metadata (not the underlying datasets themselves), documents how data can be accessed, and provides context such as ownership, update frequency, standards alignment, and quality.
Its purpose is to improve interoperability and support the Once-Only principle in public administration by reducing redundant data collection and enabling data reuse across authorities.

## Scope

This frontend exposes public catalog content and detail views for metadata objects, including:

- Datasets
- Data services
- Public services
- Concepts
- Mapping tables
- Metasearch integrations (geocat and opendata)
- Organisations
- News and home content

Routing is language-aware (`:langId`) and feature modules are lazy-loaded from `src/app`.

## Tech Stack

- Angular 21
- Angular Material + CDK
- Oblique UI (`@oblique/oblique`)
- `@ngx-translate/core` for i18n
- `angular-oauth2-oidc` for OAuth/OIDC integration
- `ngx-matomo-client` for analytics integration
- ESLint + Prettier for code quality

## Prerequisites

- Node.js `^20.19.0`, `^22.12.0`, or `>=24.0.0`, and npm installed

## Setup

```bash
npm install
```

## Run Locally

```bash
npm run start
```

Default URL: `http://localhost:4200`

The root route redirects to `de/home`.

## Build

```bash
npm run build
```

Production build:

```bash
npm run build-prod
```

Output directory: `dist/`

## Quality Checks

Lint:

```bash
npm run lint
```

Lint with autofix:

```bash
npm run lint:fix
```

Prettier formatting:

```bash
npm run prettier
```

Security audit:

```bash
npm run audit
```

Run formatting workflow (lint fix + prettier):

```bash
npm run format
```

## Architecture

Main structure:

- `src/app/app-routing.module.ts`: language-based root routing and lazy-loaded feature modules
- `src/app/catalog`: catalog list/search entry points
- `src/app/datasets`, `src/app/data-services`, `src/app/public-services`: detail views by object type
- `src/app/concepts`, `src/app/mappingtables`: additional metadata domains
- `src/app/metasearch`: external metasearch modules
- `src/app/shared`: shared components, guards, pipes, and utility helpers

When adding a feature:

- Create a dedicated feature folder in `src/app`
- Add or extend a lazy-loaded module where appropriate
- Keep reusable logic in `src/app/shared`

## Configuration

Environment files:

- `src/environments/environment.ts`
- `src/environments/environment.dev.ts`
- `src/environments/environment.prod.ts`

Static runtime config:

- `src/assets/config/appconfig.json`
- `src/assets/config/appconfig.token.json`

Build replacements (see `angular.json`):

- Production build replaces `environment.ts` with `environment.prod.ts`
- Production build replaces `appconfig.json` with `appconfig.token.json`

App config keys are defined in `src/app/app.config.interface.ts`.

Azure Static Web Apps routing and security headers are configured in `staticwebapp.config.json`.

## Troubleshooting

- App config not loading:
  - Ensure `src/assets/config/appconfig.json` exists and is valid JSON.
  - The app reads config synchronously at startup from `assets/config/appconfig.json`.
- Unexpected route behavior:
  - Confirm language-prefixed routes (for example `de/...`, `fr/...`, `it/...`, `en/...`).
  - Check redirects in `src/app/app-routing.module.ts`.
- Production-only configuration issues:
  - Verify file replacements in `angular.json` and values provided for tokenized config.

## Notes

- This repository currently defines linting and formatting scripts, but no dedicated `test` npm script in `package.json`.
