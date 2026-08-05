# Changelog

All notable changes to this repository are documented in this file.

## [Unreleased]

### Added

- Added an optional Elasticsearch search engine path for Core API search, including local Docker Compose support for Elasticsearch and Kibana.
- Added export-related capabilities in API/UI flows (including export endpoints and UI export actions).
- Added functional UI enhancements across catalog workflows, including multi-publisher filters and additional concept action controls.
- Added E2E test coverage into the monorepo test structure.

### Changed

- Updated `README.md` and `SECURITY.md` to current repository paths (`backend`, `frontend`, `build`) and current API client generation flow.
- Updated local development documentation (`README.md`, `GETTING_STARTED.md`) with Docker Compose auth behavior, local Keycloak test account usage, and forced realm reimport instructions after role/user edits.
- Updated prompt files used for documentation generation to match current repository structure:
  - `.github/prompts/create-readme.prompt.md`
  - `.github/prompts/create-third-party-licenses.prompt.md`
- Refined contribution documentation conventions (default branch wording and commit reference format) in `CONTRIBUTING.md` and `.github/prompts/create-contributing.prompt.md`.
- Migrated and stabilized monorepo structure and build/deploy workflows around the `backend` and `frontend` layout.
- Updated API client generation and frontend consumption flow around generated TypeScript client assets.

### Fixed

- Fixed Partner API handling for `ContactPoint.kind` edge cases.
- Fixed local JWT handling in Development for the Docker stack by forwarding bearer tokens between APIs and relaxing strict audience validation for local Keycloak-issued tokens.
- Fixed multiple UI behavior issues (selection/index handling, validation, and alignment-related defects).
- Fixed various build and CI workflow regressions introduced during repository restructuring.

### Security

- Patched frontend dependency vulnerabilities, including GHSA-related transitive updates and multiple dependency bumps.

## [2.0] - 2026-07-01

### Added

- Added initial monorepo components for public UI, admin UI, and IRI API integration.
- Added concept model enhancements (including the `replaces` attribute) and related UI behavior updates.

### Changed

- Introduced baseline CI/CD and deployment workflows for backend and frontend delivery.

### Fixed

- Fixed public service posting behavior and multiple deployment/configuration issues in early monorepo integration.

### Security

- Updated frontend dependency chains to address reported dependency-risk findings.
