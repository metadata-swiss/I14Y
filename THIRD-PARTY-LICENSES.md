# Third-Party Licenses

The repository-owned source code is published under the MIT License.

Third-party components remain subject to their own license terms.

## Scope

- Frontend dependency graphs:
  - src/ui/public-ui/package-lock.json
  - src/ui/admin-ui/package-lock.json
  - src/i14y/bfs-iop-admin-ui/package-lock.json
- Backend dependency graph:
  - src/i14y/\*.csproj
  - dotnet list package --include-transitive --format json

## Summary (Direct + Transitive)

- Frontend packages: 3028
- Backend packages: 205

Frontend license families:

- MIT (2433)
- ISC (232)
- Apache-2.0 (168)
- BSD-2-Clause (73)
- BlueOak-1.0.0 (53)
- BSD-3-Clause (48)
- 0BSD (6)
- CC-BY-3.0 (3)
- CC-BY-4.0 (3)
- CC0-1.0 (3)
- Python-2.0 (3)
- EPL-2.0 (2)
- LGPL-3.0-only (1)

Backend license families:

- MIT (167)
- Apache-2.0 (31)
- BSD-3-Clause (3)
- PostgreSQL (2)
- MS-PL OR Apache-2.0 (1)
- RPL-1.5 (1)

## Forbidden Licenses Policy

Blocked by policy (runtime and development scopes):

- GPL-2.0
- GPL-3.0
- LGPL-2.1
- LGPL-3.0
- AGPL-3.0
- SSPL-1.0
- MPL-2.0

Exceptions require written approval from BFS Legal and must be recorded in the project risk register.

Current status against this policy:

- Backend: no blocked license currently detected.
- Frontend: blocked license currently detected (LGPL-family via rollup-plugin-dts in i14y-bfs-iop-admin-ui dev scope).

## Key Notes

- AutoMapper (16.1.1): RPL-1.5
- rollup-plugin-dts (6.4.1): LGPL-3.0-only, introduced via ng-packagr in src/i14y/bfs-iop-admin-ui dependency graph.
- @I14Y-ch/bfs-iop-admin-web-api-client lockfile metadata omits a license field; summary keeps internal MIT classification for compatibility reporting.
- Runtime examples currently present in transitive graph include caniuse-lite (CC-BY-4.0) and elkjs (EPL-2.0).

Monitoring expectation (all runtime dependencies, not only examples above):

- Review all runtime dependencies (direct + transitive) at each release or dependency update.
- Detect any package addition/removal and any version or license change.
- Reassess risk when a runtime dependency changes license family.
- Update this file, THIRD-PARTY-DIRECT-LICENSES.md, and THIRD-PARTY-TRANSITIVE-LICENSES.md before release.

## Package Tables

- Direct dependencies table: THIRD-PARTY-DIRECT-LICENSES.md
- Transitive dependencies table: THIRD-PARTY-TRANSITIVE-LICENSES.md

## Publication Note

Publishing this repository under MIT applies only to repository-owned code.

Redistribution that includes third-party dependencies remains subject to the obligations of their respective licenses.

As of this inventory snapshot, backend blocked-license policy checks are green and frontend checks are red due to one LGPL-family transitive dependency.
