# Third-Party Licenses

The repository-owned source code is published under the MIT License.

Third-party components remain subject to their own license terms.

## Scope

- Frontend lockfiles: frontend/admin-ui/package-lock.json, frontend/public-ui/package-lock.json
- Backend inventory command: dotnet list backend/i14y.slnx package --include-transitive --format json
- Backend raw inventory artifact: tmp/backend-packages.json
- NuGet evidence path types: registration API, nuspec expression, nupkg license-file content classification, URL content classification

## Summary (Direct + Transitive)

- Frontend package rows: 2109
  - Direct: 103
  - Transitive: 2006
- Backend package rows: 205
  - Direct: 50
  - Transitive: 155

Frontend license families:

- 0BSD (4)
- Apache-2.0 (133)
- BlueOak-1.0.0 (34)
- BSD-2-Clause (50)
- BSD-3-Clause (32)
- CC-BY-3.0 (2)
- CC-BY-4.0 (2)
- CC0-1.0 (2)
- EPL-2.0 (2)
- ISC (158)
- MIT (1688)
- Python-2.0 (2)

Backend license families:

- Apache-2.0 (26)
- BSD-3-Clause (3)
- MIT (173)
- MS-PL OR Apache-2.0 (1)
- PostgreSQL (2)

## Forbidden Licenses Policy

Blocked by policy (runtime and development scopes):

- GPL-2.0
- GPL-3.0
- LGPL-2.1
- LGPL-3.0
- AGPL-3.0
- SSPL-1.0
- MPL-2.0

## Blocked Findings

- none

## Key Notes

- Internal override: none applied.
- Frontend unresolved licenses: 0
- Backend unresolved licenses: 0
- NuGet resolution counts: registration-expression=0, nuspec-expression=190, nupkg-content-classification=10, url-content-classification=5, unresolved=0

## Package Tables

- Direct dependencies table: THIRD-PARTY-DIRECT-LICENSES.md
- Transitive dependencies table: THIRD-PARTY-TRANSITIVE-LICENSES.md

## Publication Note

Publishing this repository under MIT applies only to repository-owned code.

Redistribution that includes third-party dependencies remains subject to the obligations of their respective licenses.

As of this inventory snapshot, blocked-license policy checks are green.
