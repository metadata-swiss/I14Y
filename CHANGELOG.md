# Changelog

All notable changes to this repository are documented in this file.

## [Unreleased]

### Added

- Added an Agent IRI endpoint and a GetAllAgentRelatedResources capability for agent-linked resource retrieval.
- Added concept-property inheritance behavior in concept management flows.

### Changed

- Improved Public UI behavior by displaying themes from other taxonomies with clickable links.

### Fixed

- Fixed missing translation handling in UI text rendering.
- Fixed export agents endpoint behavior.
- Fixed authentication behavior by returning HTTP 401 when tokens are expired.

### Security

- Patched high-risk frontend dependency vulnerabilities with transitive updates and package bumps.

## [2.2.0] - 2026-08-05

### Added

- Added support for selecting multiple publishers in search filters.
- Added support for multiple temporal and spatial coverages in dataset editing.
- Added export capabilities in API and UI flows, including export endpoints, UI export actions, and RDF export for agents.
- Added lock and unlock action capabilities in concept workflows.
- Added better links for non-published concepts in input flows.
- Added Partner API support for posting arrays of objects.
- Added a current-user endpoint in identity and partner-facing API flows.
- Added a workflow to create flat structures in Admin UI.

### Changed

- Replaced AutoMapper with Mapster in backend mapping flows.
- Updated selected API contracts, including query-parameter based retrieval for codelist entries.
- Refined multilingual and codelist handling behavior in UI and API responses.

### Fixed

- Fixed ContactPoint and VCard kind handling in partner dataset creation.
- Fixed missing validation and index-shifting issues in selection and save workflows.
- Fixed unlock feedback messaging and related UI alignment defects.
- Fixed SPARQL query behavior and hidden-class deletion handling in linked-data flows.
- Fixed missing error messaging when deleting agents still referenced by other objects.

### Security

- Patched frontend dependency vulnerabilities, including updates for brace-expansion, body-parser, fast-uri, postcss, and ip-address.

## [2.1.0] - 2026-07-01

### Added

- Added the IRI service module to the platform.
- Added a table view for structures with query-parameter support and graph view defaults.
- Added a replaces attribute on concepts.
- Added UI navigation improvements around back-button behavior.

### Changed

- Stabilized API client generation and publication flow for the admin client.
- Updated deployment and release wiring for public UI delivery.

### Fixed

- Fixed posting public services with channels.
- Fixed early deployment and configuration issues, including missing environment variables.

### Security

- Updated vulnerable frontend dependencies to patched versions, including sigstore and related high-risk packages.
