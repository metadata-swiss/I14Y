# Changelog

All notable changes to this repository are documented in this file.

## [Unreleased]

### Added

- Added an experimental, opt-in Elasticsearch search engine for the Core API (`Search:Engine=Elasticsearch`; default remains Lucene), covering catalog and code-list-entry search, with a local `docker-compose` stack and developer documentation under `docs/search/`.
- Added explicit repository documentation for API npm client generation flow (`src/i14y/bfs-iop-admin-ui`) and its distinction from runtime frontends (`src/ui/*`).
- Added onboarding steps for generating and building the admin API TypeScript client package.
- Added dependency-risk wording to the security policy for third-party components.

### Changed

- Refreshed root documentation set (`README.md`, `GETTING_STARTED.md`, `CONTRIBUTING.md`, `SECURITY.md`) for consistency with current monorepo structure and workflows.
- Clarified that package publishing statements must remain evidence-based.

### Notes

- The Elasticsearch client packages are Apache-2.0, but the Elasticsearch **server** (9.x) is tri-licensed AGPL-3.0/ELv2/SSPL-1.0 — AGPL-3.0 and SSPL-1.0 are both blocked by policy and ELv2 is not OSI-approved. It is used only as an optional, dev-only local dependency; a policy-compliant runtime (e.g. Apache-2.0 OpenSearch) must be selected before any production adoption. See `THIRD-PARTY-LICENSES.md`.
- Release-version history entries will be added when maintainers publish tagged releases and define support windows.
