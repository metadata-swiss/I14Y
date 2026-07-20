# Third-Party Licenses

The repository-owned source code is published under the MIT License.

Third-party components remain subject to their own license terms.

## Scope

- Frontend dependency graphs:
  - src/frontend/admin-ui/package-lock.json
  - src/frontend/public-ui/package-lock.json
  - src/i14y/bfs-iop-admin-ui/package-lock.json
- Backend dependency graph:
  - src/i14y/*.csproj
  - dotnet list package --include-transitive --format json
  - NuGet registration + nuspec + nupkg license-file content classification

## Summary (Direct + Transitive)

- Frontend package rows: 3084
  - Direct: 118
  - Transitive: 2966
- Backend package rows: 207
  - Direct: 48
  - Transitive: 159

Frontend license families:

- MIT (2473)
- ISC (238)
- Apache-2.0 (180)
- BSD-2-Clause (73)
- BlueOak-1.0.0 (51)
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
- Apache-2.0 (33)
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
- Frontend: blocked license currently detected.

## Blocked Findings

- frontend | i14y-bfs-iop-admin-ui | rollup-plugin-dts@6.4.1 | LGPL-3.0-only | dev | transitive

## Key Notes

- AutoMapper (16.1.1): resolved via package license-file content classification.
- @I14Y-ch/bfs-iop-admin-web-api-client lockfile metadata omits a license field; summary applies explicit internal override mapping UNKNOWN -> MIT for compatibility reporting (internal package only).
- Frontend unresolved licenses after override mapping: 0.
- Backend unresolved licenses after NuGet + content classification: 0.
- Elasticsearch integration (optional, disabled by default — `Search:Engine=Lucene`): the NuGet client `Elastic.Clients.Elasticsearch` (8.15.10) and its transport `Elastic.Transport` (0.4.26) are Apache-2.0 and are included in the tables above. The Elasticsearch **server** itself (run via docker-compose for local dev only) is licensed ELv2 / **SSPL-1.0** — SSPL-1.0 is on this repository's blocked list. It is a runtime infrastructure component (like a database server), not a NuGet/npm dependency in the graphs above, so it is not a package row; it is recorded here for policy visibility. For a policy-compliant runtime, use the Apache-2.0 licensed **OpenSearch** (wire-compatible with the client) instead of the Elastic distribution before any production adoption.

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

As of this inventory snapshot, blocked-license policy checks are red due to entries listed above.
