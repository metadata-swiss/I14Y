---
agent: "agent"
description: "Create or update root sbom.spdx.json (SPDX 2.3 JSON SBOM) for the I14Y monorepo"
argument-hint: "Optional overrides: include dev dependencies (yes/no), include NuGet coverage (yes/no), SPDX document namespace override, execution mode (one-shot)"
---

## Role

You're a software supply-chain engineer focused on evidence-based SBOM generation using the SPDX 2.3 specification.

## Task

Create or update the root Software Bill of Materials (SBOM) for this monorepo as a single SPDX 2.3 JSON document:

- root `sbom.spdx.json` (SPDX 2.3, `dataLicense` = `CC0-1.0`)

Evidence collection is delegated. Before rendering any SPDX JSON:

1. Read `.github/skills/dependency-license-evidence/SKILL.md` and follow it to produce or refresh the shared `tmp/` artifacts (`tmp/backend-packages.json`, `tmp/backend-package-licenses.json`, `tmp/frontend-transitive-licenses.csv`, `tmp/transitive-license-summary.json`).
2. Use only those `tmp/` artifacts as the source of truth for `packages[]` entries. Do not re-run inventory logic here; do not reshape evidence; do not re-classify licenses independently of the skill.
3. If the skill reports the run as blocked (for example the mandatory backend inventory command fails), STOP and report the run as blocked. Do not preserve a previous `sbom.spdx.json` as if regeneration succeeded.

SPDX 2.3 output rules (this workflow only):

1. Regenerate `sbom.spdx.json` on every run so it stays aligned with the freshly produced `tmp/` artifacts.
2. Emit exactly one SPDX document describing the whole monorepo. Do not split into multiple SBOM files. Do not include a `files[]` inventory; this SBOM is package-level only (`filesAnalyzed: false`).
3. Required top-level SPDX fields:
   - `spdxVersion`: `"SPDX-2.3"`
   - `dataLicense`: `"CC0-1.0"`
   - `SPDXID`: `"SPDXRef-DOCUMENT"`
   - `name`: `"I14Y"` (or the repository name if different)
   - `documentNamespace`: `https://github.com/<owner>/<repo>/sbom/<suffix>`. `<suffix>` MUST be the current git HEAD commit SHA when available (stable, reproducible: identical commit ⇒ identical namespace). Fall back to a compact UTC timestamp (`yyyyMMddTHHmmssZ`) only when git metadata is not reachable. Do not use a random UUID.
   - `documentComment`: MUST be present and MUST cross-reference the committed license inventory files (`THIRD-PARTY-LICENSES.md`, `THIRD-PARTY-TRANSITIVE-LICENSES.md`). MUST also state the relationship-modeling limitation whenever parent-child edges are not derived (see rule 8). This makes the two Markdown files the authoritative human-readable source of direct-vs-transitive classification and prevents consumers from misinterpreting flat `DEPENDS_ON` edges.
   - `creationInfo.created`: ISO 8601 UTC timestamp of this run
   - `creationInfo.creators`: at minimum `Organization: <repository owner declared in publiccode.yml>` (for I14Y: `Organization: Federal Statistical Office (FSO), Switzerland`). Do not add a `Tool:` entry referencing the AI assistant that ran the workflow (`github-copilot-agent`, model names, etc.). A `Tool:` entry is acceptable only if it names the deterministic script that emitted the JSON (for example `Tool: i14y-sbom-generator`).
   - `creationInfo.licenseListVersion`: value from `tmp/transitive-license-summary.json` `spdxLicenseListVersion` when non-null; otherwise omit
4. Emit a root package that represents the repository itself and link it via a `DESCRIBES` relationship from `SPDXRef-DOCUMENT`.
5. Emit one SPDX `packages[]` entry per unique `(ecosystem, name, version)` across:
   - every row of `tmp/frontend-transitive-licenses.csv` (frontend direct and transitive, both UIs)
   - every entry of `tmp/backend-package-licenses.json` (backend direct and transitive, already deduplicated per the skill's backend uniqueness rule)
6. For each package entry, populate SPDX fields from the shared artifacts:
   - `SPDXID`: unique `SPDXRef-Pkg-<ecosystem>-<sanitized-name>-<version>` identifier matching `^SPDXRef-[A-Za-z0-9.\-]+$`
   - `name`: package name from the artifact row
   - `versionInfo`: version from the artifact row
   - `downloadLocation`: `downloadLocation` from `tmp/backend-package-licenses.json` for NuGet; the resolved npm registry tarball URL from the lockfile for npm; otherwise `NOASSERTION`
   - `filesAnalyzed`: `false` (this SBOM is package-level inventory only; the generator does not open archives, hash individual files, or emit `SPDXRef-File` entries. Do not switch to `true` without also emitting the file inventory and `packageVerificationCode` for every package.)
   - `licenseConcluded`: `spdxLicense` from the artifact when it is a valid SPDX identifier or expression; `NOASSERTION` when the artifact value is `UNKNOWN`
   - `licenseDeclared`: same as `licenseConcluded` unless the artifact distinguishes them; `NOASSERTION` when `UNKNOWN`
   - `copyrightText`: `NOASSERTION` (this workflow does not extract copyright notices; SPDX 2.3 §7.16 requires the field to be present, and `NOASSERTION` is the honest value when no extraction was attempted)
   - `supplier`: OPTIONAL per SPDX 2.3 §7.5. Emit `Organization: <publisher>` only when a real publisher is available:
     - NuGet packages: use `publisher` from `tmp/backend-package-licenses.json` (nuspec `<authors>`).
     - npm packages: use the `Publisher` column from `tmp/frontend-transitive-licenses.csv` (parsed from `node_modules/<pkg>/package.json` `author`/`maintainers` by the skill).
     - When the artifact publisher is empty or `UNKNOWN`, OMIT the `supplier` field entirely. Do not emit `"supplier": "NOASSERTION"` as a placeholder.
   - `externalRefs`: include a `PACKAGE-MANAGER` reference of type `purl` with a valid Package URL (`pkg:npm/<name>@<version>`, `pkg:nuget/<name>@<version>`)
7. Never emit raw placeholders like `LICENSE-FILE:<path>` or `LICENSE-URL:<url>` — the skill has already resolved them to SPDX or `UNKNOWN`. Map `UNKNOWN` to `NOASSERTION` and stop there.
8. Emit `relationships[]` covering at minimum:
   - `SPDXRef-DOCUMENT` `DESCRIBES` the root repository package
   - Root repository package `DEPENDS_ON` each direct frontend and backend package
   - Each direct backend package `DEPENDS_ON` its transitive backend packages when parent linkage is available in `tmp/backend-packages.json`; otherwise attach transitive backend packages to the root repository package via `DEPENDS_ON` and note the fallback in the run report
   - Each direct frontend package `DEPENDS_ON` its transitive frontend packages when parent linkage is available in the lockfile; otherwise attach transitive frontend packages to the root repository package via `DEPENDS_ON` and note the fallback in the run report
9. Ensure JSON is valid and stable:
   - UTF-8 encoded
   - two-space indentation
   - trailing newline at end of file
   - deterministic ordering: sort `packages[]` by `SPDXID`, sort `relationships[]` by `(spdxElementId, relationshipType, relatedSpdxElement)`
10. Execute the full SBOM generation in one run and do not stop after partial updates. Do not ask for confirmation between steps when evidence is sufficient; complete the SBOM first, then report results.

SPDX-specific consistency checks (fail the run when any fails):

- Total `packages[]` count minus the root repository package MUST equal `uniquePackageCounts.total` from `tmp/transitive-license-summary.json` (frontend deduplicated across UIs on `(name, version)`, backend already deduplicated by the skill's backend uniqueness rule).
- Frontend `packages[]` count MUST equal `uniquePackageCounts.frontend`.
- Backend `packages[]` count MUST equal `uniquePackageCounts.backend`.
- Every `packages[]` entry MUST have a `purl` external reference.
- Every `SPDXID` MUST be unique and match the pattern `^SPDXRef-[A-Za-z0-9.\-]+$`.
- Every `spdxElementId` and `relatedSpdxElement` in `relationships[]` MUST resolve to a declared `SPDXID`.
- JSON MUST parse without errors.

## Output

After editing, provide:

1. Path of the generated SBOM and its SPDX version.
2. Ecosystem coverage counts (frontend direct/transitive, backend direct/transitive) and confirmation they match `tmp/transitive-license-summary.json`.
3. Whether the skill reused existing `tmp/` evidence or bootstrapped fresh evidence. State explicitly whether the run started from an empty `tmp/` (fresh clone bootstrap) or from pre-existing artifacts. Include the exact backend inventory command executed and every `tmp/` artifact path produced.
4. Number of packages emitted with `licenseConcluded` as `NOASSERTION` and the reason breakdown from the skill's `unresolved` counters and `overrides` list.
5. Number of relationships emitted, split by `relationshipType`.
6. Any dependency parent linkage limitations that forced fallback to root-level `DEPENDS_ON`.
7. Validation checks executed and their pass/fail status.

## Constraints

- Do not invent package names, versions, licenses, suppliers, or download URLs.
- If evidence is missing at SBOM rendering time, the skill has already marked it `UNKNOWN` — map to `NOASSERTION` in the SPDX field and do not guess.
- Prefer root-level `sbom.spdx.json` as the single SBOM output; do not emit per-project SBOMs.
- Raw placeholders such as `LICENSE-FILE:<path>` or `LICENSE-URL:<url>` are not acceptable values in SPDX license fields; use the classified SPDX identifier from the skill or `NOASSERTION`.
- Treat this as a one-shot workflow: partial completion is not acceptable when inputs are available.
- Do not re-implement the evidence collection or NuGet resolution chain in this prompt; follow the skill.
- Do not rely on `build/` for temporary generated evidence; the skill uses `tmp/` for ephemeral files that should not be committed. The final `sbom.spdx.json` itself lives at the repository root and is a committed artifact.
- Do not include file-level (`files[]`) inventory; this SBOM is package-level only (`filesAnalyzed: false`).
