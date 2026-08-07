# Changelog

All notable changes to this repository are documented in this file.

## [Unreleased]

### Added

- Added export capabilities across API and UI flows, including export endpoints, UI export actions, and RDF export for agents.
- Added catalog workflow enhancements, including multi-publisher filters, multiple temporal and spatial coverage values, and additional concept lock/unlock actions.
- Added partner and identity API capabilities, including current-user retrieval and expanded partner post-array endpoints.
- Added E2E test coverage into the monorepo test structure.

### Changed

- Stream `2.1` (`2.1.0` to `2.1.59`) is tracked here because only lightweight tags are available and no publish metadata was found.
- Updated repository and local-development documentation to reflect the current monorepo layout (`backend`, `frontend`, `build`) and Docker Compose usage.
- Migrated and stabilized monorepo structure and build/deploy workflows around the current backend/frontend layout.
- Updated API client generation and frontend consumption flow around generated TypeScript client assets.

### Fixed

- Fixed Partner API handling for `ContactPoint.kind` edge cases.
- Fixed SPARQL query behavior and hidden class deletion handling in linked-data flows.
- Fixed multiple UI behavior issues (selection/index handling, validation, unlock messaging, and alignment defects).
- Fixed various build and CI workflow regressions introduced during repository restructuring.

### Security

- Patched frontend dependency vulnerabilities, including GHSA-related transitive updates and dependency bumps for `brace-expansion`, `body-parser`, `fast-uri`, `postcss`, `ip-address`, and related packages.

## [2.0] - 2026-06-30

### Added

- Added initial monorepo components for public UI, admin UI, and IRI API integration.
- Added concept model enhancements (including the `replaces` attribute) and related UI behavior updates.

### Changed

- Introduced baseline CI/CD and deployment workflows for backend and frontend delivery.
- Established initial API client generation and package publication wiring used by the monorepo delivery flow.

### Fixed

- Fixed public service posting behavior and multiple deployment/configuration issues in early monorepo integration.

### Security

- Updated frontend dependency chains to address reported dependency-risk findings.
