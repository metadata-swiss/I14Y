# Changelog

All notable changes to this repository are documented in this file.

## [2.5.0] - 2026-11-04

### Added

- Added support for EU data themes alongside I14Y themes, with a theme selector grouped by taxonomy in Admin UI forms and EU themes assigned to existing content based on the I14Y theme mapping.
- Added license and keyword filters to catalog search.

### Changed

- Catalog search now runs on a dedicated search service.
- Search ranking now gives additional weight to frequently reused concepts.
- Search results with equal relevance are now ordered by registration status for all roles.
- Removed the maximum result limit on search endpoints.
- Search scoring parameters are now configurable.
- Catalog search results in Admin UI now show the taxonomy name next to each theme.
- Themes are now set on datasets and data services instead of catalog records; the Partner API rejects catalog records that still contain themes.
- Replaced sub-agent relationships with "sub organization of" relationships for agents, including in RDF exports.
- A contact point is now mandatory for agents.
- Public UI dataset and concept descriptions no longer wait for LINDAS content to load, and LINDAS sections show loading indicators with tooltips.
- Improved the performance of linked-resource tables in Admin UI edit forms for public services and datasets.

### Fixed

- Fixed resources remaining authorized against a previous agency by preventing changes to agent identifiers after creation.
- Fixed associations not being displayed in structure graphs.
- Fixed codelist entry search results showing only the first parent code in the hierarchy path.
- Fixed a validation error on the catalog page.
- Fixed mapping table export file names starting with "Dataservice_" instead of "MappingTable_".
- Fixed the outdated GitHub URL in the Admin UI footer.
- Fixed missing translations in Admin UI, including the structure detail editor.
- Fixed issues in audit trail change recording.

### Security

- Public search no longer matches non-public fields, such as data owner and responsible-person contact details, for unauthenticated users.
- Updated Admin UI and Public UI npm dependencies with high-risk vulnerabilities.

## [2.4.0] - 2026-09-30

### Added

- Added an audit trail that records changes to resources, with a Core API endpoint to retrieve the change history.
- Added filtering by qualified attribution to catalog search in Admin UI, Admin API and Partner API.
- Added "sub agent of" relationships to agents, with an Admin API endpoint to retrieve them.

### Changed

- Search filters now only list agents that have at least one matching result.
- Improved responsive layouts in both user interfaces.
- CSV exports of codelist entries and mapping relations now use semicolons as delimiter.

### Fixed

- Fixed slow loading of concept relations.
- Fixed theme fallback display for datasets with multiple themes.
- Fixed missing translations in Admin UI and in Public UI search filters and result tables.
- Fixed failures for machine-to-machine API clients whose tokens contain no email address, and timeouts when tracking resource changes.
- Fixed CSV import of codelist entries and mapping relations, including semicolon-delimited files and empty multilingual values.

### Security

- Resolved high-risk vulnerabilities in Admin UI and Public UI dependencies.

## [2.3.0] - 2026-09-09

### Added

- Added an About dialog in both user interfaces.
- Added configurable response caching for public API retrieval endpoints.
- Added Agent IRI links in public metadata descriptions.

### Changed

- Updated dataset information shown in Admin UI catalog views.
- Harmonized typography across both user interfaces with design-system defaults.

### Fixed

- Fixed duplicate tables displayed when creating a structure from scratch.
- Fixed incorrect concept relation counts.
- Fixed identifier input handling for data services and public services in Admin UI.
- Fixed structure-detail field overrides during editing.

### Security

- Updated vulnerable frontend and backend dependencies.

## [2.2.0] - 2026-08-26

### Added

- Added clickable links to themes from other taxonomies in Public UI catalog entries.
- Added endpoints to retrieve agent IRIs and agent-related resources.
- Added a control to inherit properties from concepts linked to structure attributes.
- Added links from public concept and dataset pages to matching LINDAS content.

### Changed

- Improved editing of temporal and spatial coverage for dataset distributions.
- Improved the presentation of external links across both user interfaces.
- Added deterministic ordering to RDF structure exports.

### Fixed

- Fixed missing translations in Public UI search results.
- Fixed agent export endpoints.
- Returned HTTP 401 responses when access tokens have expired.
- Suppressed user-visible errors when no matching LINDAS content is available and corrected LINDAS concept queries.
- Fixed access URLs for distributions in RDF catalog exports.
- Fixed language fallback for organization names.
- Improved error details shown for failed requests and database errors.

### Security

- Updated frontend dependencies to address high-risk vulnerabilities.

## [2.1.0] - 2026-08-05

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

## [2.0.0] - 2026-07-01

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
