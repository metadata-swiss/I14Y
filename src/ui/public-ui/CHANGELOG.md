# Changelog

## [1.4] - 2026-05-21

### Added

- Displayed URIs for classes and attributes on structure detail pages.
- Added system metadata display.
- Added concept reuse visibility, including reuse counts on the catalog page.
- Added larger graph display support and improved graph search behavior.

### Changed

- Replaced external links to controlled vocabularies.
- Improved public distributions table presentation.
- Improved mapping table display on concept detail pages.

### Fixed

- Fixed multiple layout issues across detail pages, organization pages, and catalog pages.
- Fixed URI version fallback consistency between mapping tables and structures.
- Fixed popup behavior and formatting issues in distribution columns.
- Fixed internal link behavior to avoid opening internal routes in new tabs.
- Fixed structure connection rendering to match admin-side behavior.
- Fixed landing page header/title rendering on mobile devices.
- Fixed stale concept version state when switching resources.
- Fixed vocabulary link version handling in popup views.

### Removed

- Removed obsolete legacy IOP file.

## [1.3] - 2026-04-29

### Added

- Added a mapping table view and integrated mapping tables into catalog, concept, and organization pages.
- Added IRI display for mapping table content and made attribute IRI links clickable.
- Added codelist download without annotations.
- Added vocabulary-based attribute value links.
- Added identifier and URI alignment improvements for public services and concepts.

### Changed

- Improved endpoint and version display behavior, including descending version ordering in relevant views.
- Improved naming/label display for code systems, including disambiguation by version when needed.
- Moved distribution content into the description area where appropriate.

### Fixed

- Fixed slow metasearch behavior on opendata.swiss.
- Fixed data service and public service URL routing based on identifiers.
- Fixed codelist entry handling for hash-based IRIs and escaped HTML rendering.
- Fixed missing access service links on distribution pages.
- Fixed fallback behavior for unresolved URIs in structures.
- Fixed empty-keyword fallback to display URI values.
- Fixed sidebar/table/layout regressions introduced by dependency updates.
- Fixed translation and public layout inconsistencies.
- Fixed incorrect start-page contact email.

### Security

- Applied dependency audit fixes, including high-risk dependency remediation.

## [1.2] - 2026-02-12

### Added

- Added URI setup and display for datasets, data services, and public services.
- Added URI display on concept detail pages and codelist entries.
- Added structure filter support.
- Added unit property and target class support.

### Changed

- Adjusted filter ordering for better usability.
- Corrected endpoint URL and endpoint description presentation.

### Fixed

- Fixed codelist entry search behavior.
- Fixed fallback handling for class URIs.
- Fixed browser tab name harmonization behavior.
- Fixed icon display when metadata is unavailable.
- Fixed validation behavior related to minimum length undo changes.
- Fixed concept IRI composition and concept IRI pattern configuration.
- Fixed sorting behavior in sortable list views.
- Fixed visibility of non-public `conformsTo` links.

### Security

- Applied npm audit fixes.
