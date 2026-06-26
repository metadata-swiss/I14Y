# Changelog

## 1.5 - 2026-05-20

### Added
- Links from concept pages to related mapping tables.
- A workflow to create new versions of mapping tables.
- Reuse visibility for concepts in catalog and related dataset views.
- System metadata visibility, including warnings for automated creation contexts.
- Info-popover based links for controlled vocabularies on detail pages.

### Changed
- Upgraded core frontend stack (Angular, Material, TypeScript, Oblique) and related auth/client wiring.
- Improved structure editing behavior, table expansion behavior, and detail page layout consistency.
- Aligned URI and fallback behavior across mapping tables, concepts, and structures.
- Improved mapping table display on concept detail pages and catalog views.

### Fixed
- Fixed concept creation and save flows, including identifier/version edge cases.
- Fixed mapping table publication links, popup behavior, duplicate connection identifiers, and edit page spacing.
- Fixed role and user info display issues in dashboard and detail views.
- Fixed multiple fallback and URI rendering defects (including unresolved URI version display and structure fallback).
- Fixed metadata banner placement, keyword/annotation messaging, and icon/status rendering issues.

## 1.4 - 2026-03-26

### Added
- Mapping table edit support and deeper mapping table integration in catalog/navigation.
- Display of IRI details for codelist entries and improved concept/code system presentation.
- Optional export flag to download codelist entries without annotations.

### Changed
- Enabled publisher changes on concepts.
- Made selected identifier fields optional where required.
- Ordered versions in descending order to improve latest-version visibility.
- Improved contact person/deputy handling and related UI translations.

### Fixed
- Fixed codelist entry IRI link behavior when values include `#`.
- Fixed auth role checks in guards/session-related flows and dashboard role checks.
- Fixed validation for keyword entries when label and URI are both missing.
- Fixed cases where IRI values were hidden under sparse additional-information payloads.

### Removed
- Removed calls to obsolete backend endpoints.

## 1.3 - 2026-02-12

### Added
- Custom language dropdown behavior for vocabulary-related selections.
- Structure filtering capabilities in relevant views.
- Mapping table detail page.
- IRI setup support for datasets, data services, and public services.

### Changed
- Added stricter identifier regex validation.
- Improved sorting behavior in list/detail contexts.
- Improved translation wording in affected UI labels.

### Fixed
- Fixed incorrect composed IRI generation for concepts.

## 1.2 - 2026-01-22

### Added
- Token-expiry dialog handling.
- URI visibility on concept detail pages.
- Target class URI handling in relevant forms.
- Distribution data-size display improvements (including unit conversion behavior).

### Changed
- Refined concept identifier/version validation behavior.
- Improved sidebar fallback/shape handling and filter ordering behavior.
- Updated URL submission behavior to send `null` instead of empty strings where required by backend contracts.

### Fixed
- Fixed edit/modify errors when labels are undefined.
- Fixed save flow issues in dataset distribution and related validation paths.
- Fixed validator blocking errors and translation file issues.
- Addressed multiple dependency/audit risk fixes affecting runtime safety.