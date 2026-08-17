---
name: dependency-license-evidence
description: Gather and classify third-party dependency license evidence for the I14Y monorepo (npm frontend across both UIs, NuGet backend across the full solution). Produces the shared `tmp/` artifacts that downstream reporting workflows (`THIRD-PARTY-LICENSES.md`, `THIRD-PARTY-TRANSITIVE-LICENSES.md`, `sbom.spdx.json`) consume. Activates when a task requires building, refreshing, or validating dependency license inventory, running the backend NuGet inventory, classifying NuGet licenses to SPDX identifiers, or reading `tmp/transitive-license-summary.json` and its sibling evidence files.
---

## Purpose

This skill defines the single source of truth for collecting and classifying dependency license evidence for the I14Y monorepo. Consumer workflows (third-party licensing markdown, SPDX SBOM, any future audit or export) MUST follow these rules and read from the shared `tmp/` artifacts produced here. Do not re-implement the collection logic in the consumer prompt.

## Sources in scope

Frontend (npm), both UIs:

- `frontend/public-ui/package-lock.json`
- `frontend/admin-ui/package-lock.json`

Backend (NuGet), full solution:

- `backend/i14y.slnx` (and every `*.csproj` reachable from it)
- `Directory.Packages.props` when present

Every consumer run MUST cover both ecosystems. Do not omit an ecosystem because "nothing changed"; run detection is based on freshness (see below), not on user assertion.

## Bootstrap on gitignored `tmp/`

`tmp/` is gitignored. On a fresh clone none of the `tmp/*` evidence files exist. The consumer workflow MUST:

1. Detect missing evidence: any of the artifacts listed in "Shared `tmp/` artifact contract" below does not exist on disk.
2. Detect stale evidence: `tmp/transitive-license-summary.json` `generatedAt` is older than the most recent modification time of any file in "Sources in scope".
3. If missing or stale, execute the full evidence collection defined here in the same run, before producing any consumer-specific output. Do not skip. Do not fabricate. Do not silently reuse stale evidence.

## Backend inventory execution (mandatory each run)

Execute exactly:

```
dotnet list backend/i14y.slnx package --include-transitive --format json
```

Persist the raw output verbatim to `tmp/backend-packages.json`. If the command fails (non-zero exit, malformed JSON, empty projects list), STOP the run and report it as blocked. Do not preserve a previous `tmp/backend-packages.json` as if the run succeeded.

## Frontend inventory execution (mandatory each run)

Traverse both lockfiles listed in "Sources in scope" and produce a flat inventory per `(component, package, version, scope, dependencyType)` where:

- `component` is `admin-ui` or `public-ui`
- `scope` is `dev` or `runtime` (derive from lockfile `dev` flags)
- `dependencyType` is `direct` or `transitive` (direct = listed in root `dependencies` or `devDependencies`)

Include direct and transitive rows. Emit the inventory to `tmp/frontend-transitive-licenses.csv` with header:

```
"Component","ProjectName","Package","Version","Homepage","SPDX","LicenseLink","Scope","DependencyType","Publisher"
```

`ProjectName` mirrors `Package` for npm rows. `Homepage` MUST be a resolved URL (registry, repository, or homepage field) or `UNKNOWN`. `LicenseLink` MUST be the resolved SPDX license page or the package license URL, or `UNKNOWN`. `Publisher` MUST be the human-readable name from the installed package's `package.json` (`author` field, else first `maintainers` entry), or empty string when not resolvable from local `node_modules`. Do not fabricate publisher values from registry lookups in this workflow.

## NuGet license resolution chain (deterministic, per package+version)

For every backend package+version (direct or transitive), resolve license evidence in this exact order and record which step produced the result:

1. `registration-expression` — NuGet registration API returns a `licenseExpression` (SPDX). Keep it.
2. `nuspec-expression` — `.nuspec` `<license type="expression">` value is an SPDX identifier or expression. Keep it.
3. `nupkg-content-classification` — `.nuspec` points to `<license type="file">`. Download the `.nupkg`, read the referenced file, and classify to a SPDX identifier using the deterministic text fingerprints below.
4. `url-content-classification` — only a `<licenseUrl>` is available. Fetch the license text and classify using the same fingerprints.
5. `unresolved` — none of the above yields a SPDX identifier. Record `UNKNOWN` and add to the unresolved counter.

Deterministic SPDX fingerprints supported (extend only when adding new evidence):

- MIT
- Apache-2.0
- BSD-2-Clause
- BSD-3-Clause
- ISC
- MPL-2.0
- RPL-1.5

Never infer a license from the package name, publisher, or family. Never keep raw placeholders like `LICENSE-FILE:<path>` or `LICENSE-URL:<url>` in the persisted artifacts — either the SPDX identifier is resolved, or the value is `UNKNOWN`.

## Frontend license resolution

For each npm package row:

- Prefer the `license` field from the resolved package metadata when it is an SPDX identifier or expression.
- Prefer `licenses[]` when the manifest uses the legacy array form; join into an SPDX expression when unambiguous.
- Fall back to `UNKNOWN` if neither is a valid SPDX identifier/expression. Do not attempt content classification for npm packages in this workflow.

## SPDX identifier discipline

- Use the SPDX License List identifiers verbatim (`MIT`, `Apache-2.0`, `BSD-3-Clause`, `LGPL-3.0-or-later`, ...).
- When a package declares a compound license, keep the SPDX expression (`MIT OR Apache-2.0`, `(MIT AND BSD-3-Clause)`).
- Record the SPDX License List version used for classification so consumers can echo it in their outputs.
- Never downgrade a resolved SPDX identifier to `UNKNOWN`/`NOASSERTION` on subsequent passes.

## Blocked license detection (family variants)

Apply blocked-license checks to direct AND transitive rows, in runtime AND dev scopes. Do not rely on exact SPDX string equality:

- Detect family variants: `LGPL-3.0`, `LGPL-3.0-only`, `LGPL-3.0-or-later` all count as LGPL-3.0.
- Detect the same family across ecosystems.
- Detect compound expressions where any operand is blocked (`MIT OR GPL-3.0-only` under an `OR` policy is not blocked because MIT satisfies it; under an `AND` policy it is blocked — pick the strict interpretation unless overridden).

Every blocked hit is recorded with package, version, scope, ecosystem, and evidence source (which step of the resolution chain produced the SPDX identifier).

## Backend uniqueness rule

If the same backend package+version appears in both direct and transitive across the solution, keep it ONLY in direct and remove it from transitive. Reflect this deduplication in every counter and every downstream artifact. Consumer prompts MUST NOT re-add the transitive occurrence.

## Internal / private packages

When a package is internal or private and license metadata is incomplete:

- Keep it clearly separated from third-party rows in the persisted artifacts.
- If an internal override is applied (for example `UNKNOWN -> MIT` for a private component), record the override with target package+version and the mapping applied under a top-level `overrides` block in `tmp/transitive-license-summary.json`.

## Shared `tmp/` artifact contract

Every run of the evidence collection MUST produce or refresh all of the following. Consumers read only from these files.

### `tmp/backend-packages.json`

Raw verbatim output of the mandatory `dotnet list ...` command. Do not modify.

### `tmp/frontend-transitive-licenses.csv`

Flat frontend inventory as described in "Frontend inventory execution". Header exact. One row per `(component, package, version, scope)`. The `Publisher` column is empty when the local `node_modules/<pkg>/package.json` was missing or contained no usable `author`/`maintainers` value.

### `tmp/backend-package-licenses.json`

Flat backend inventory. Array of objects with this shape:

```
{
  "id": "<package id>",
  "version": "<resolved version>",
  "scope": "direct" | "transitive",
  "projects": ["<csproj path>", ...],
  "spdxLicense": "<SPDX identifier or expression, or UNKNOWN>",
  "licenseSource": "registration-expression" | "nuspec-expression" | "nupkg-content-classification" | "url-content-classification" | "unresolved",
  "licenseUrl": "<resolved URL or UNKNOWN>",
  "homepage": "<resolved URL or UNKNOWN>",
  "publisher": "<publisher/authors or UNKNOWN>",
  "downloadLocation": "<resolved .nupkg URL or UNKNOWN>"
}
```

Backend uniqueness rule applies: no entry with `scope = transitive` for a `(id, version)` that also has `scope = direct`.

### `tmp/transitive-license-summary.json`

Machine-readable summary. Required shape:

```
{
  "generatedAt": "<ISO 8601 timestamp of this run>",
  "spdxLicenseListVersion": "<version or null>",
  "backendInventoryCommand": "dotnet list backend/i14y.slnx package --include-transitive --format json",
  "backendInventoryArtifact": "tmp/backend-packages.json",
  "counts": {
    "frontendDirect": <int>,
    "frontendTransitive": <int>,
    "backendDirect": <int>,
    "backendTransitive": <int>,
    "backendTotal": <int>,
    "blocked": <int>,
    "unknown": <int>
  },
  "uniquePackageCounts": {
    "frontend": <int>,
    "backend": <int>,
    "total": <int>
  },
  "checks": {
    "backendOverlapCount": 0,
    "backendDirectMatchesSummary": true,
    "backendTransitiveMatchesSummary": true,
    "backendTotalMatchesSummary": true
  },
  "unresolved": {
    "frontendUnknownSpdx": <int>,
    "backendUnknownSpdx": <int>
  },
  "blocked": [
    {
      "ecosystem": "npm" | "nuget",
      "package": "<name>",
      "version": "<version>",
      "scope": "direct" | "transitive",
      "runtimeScope": "runtime" | "dev",
      "spdxLicense": "<identifier>",
      "evidence": "<licenseSource for backend, lockfile path for frontend>"
    }
  ],
  "nugetResolution": {
    "registration-expression": <int>,
    "nuspec-expression": <int>,
    "nupkg-content-classification": <int>,
    "url-content-classification": <int>,
    "unresolved": <int>
  },
  "overrides": [
    {
      "ecosystem": "npm" | "nuget",
      "package": "<name>",
      "version": "<version>",
      "from": "<original SPDX or UNKNOWN>",
      "to": "<applied SPDX>",
      "reason": "<free-form, must be auditable>"
    }
  ]
}
```

Consumer workflows MUST NOT reshape this schema; extending it is allowed only if all consumers are updated in the same change.

## Freshness detection algorithm

To decide whether to re-run collection:

1. If any of the four `tmp/*` artifacts above is missing: evidence is missing → collect.
2. Read `tmp/transitive-license-summary.json` `generatedAt`.
3. Compute the maximum modification time across every file in "Sources in scope".
4. If step 3 is newer than step 2: evidence is stale → collect.
5. Otherwise: evidence is fresh → consumers may proceed to their own output rendering without re-collection.

## Shared consistency checks (fail the run when any fails)

These checks are enforced by the evidence collection itself and re-checked by every consumer against `tmp/transitive-license-summary.json`:

- `checks.backendOverlapCount` MUST equal `0`.
- `counts.backendDirect` MUST equal the number of `scope = direct` entries in `tmp/backend-package-licenses.json`.
- `counts.backendTransitive` MUST equal the number of `scope = transitive` entries in `tmp/backend-package-licenses.json`.
- `counts.backendTotal` MUST equal `counts.backendDirect + counts.backendTransitive`.
- `counts.frontendDirect + counts.frontendTransitive` MUST equal the number of rows in `tmp/frontend-transitive-licenses.csv` minus the header.
- `uniquePackageCounts.frontend` MUST equal the number of unique `(name, version)` tuples across all frontend CSV rows.
- `uniquePackageCounts.backend` MUST equal `counts.backendTotal` (backend inventory is already deduplicated by the backend uniqueness rule).
- `uniquePackageCounts.total` MUST equal `uniquePackageCounts.frontend + uniquePackageCounts.backend`.
- `nugetResolution` values MUST sum to `counts.backendDirect + counts.backendTransitive`.
- Every `blocked[]` entry MUST reference a `(package, version, scope)` that exists in the corresponding inventory artifact.

## Operational discipline

- One-shot: complete the collection bundle in a single run before yielding to the consumer output stage.
- Do not ask for confirmation between artifacts.
- Do not rely on `build/` for temporary generated evidence. Use `tmp/` exclusively.
- Do not commit `tmp/` outputs (they are gitignored by design).
- Do not invent package names, versions, licenses, suppliers, homepages, or download URLs — the run is evidence-backed at package+version level or it is not run at all.
