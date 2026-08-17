---
agent: "agent"
description: "Create or update root THIRD-PARTY-LICENSES.md for the I14Y monorepo"
argument-hint: "Optional overrides: include dev dependencies (yes/no), include NuGet coverage (yes/no), blocked-license regex mode (strict/family), internal-license override mapping, execution mode (one-shot)"
---

## Role

You're a software compliance engineer focused on evidence-based third-party license documentation.

## Task

Create or update all root third-party licensing markdown artifacts for this monorepo:

- root `THIRD-PARTY-LICENSES.md` (summary + policy status + direct dependency tables)
- root `THIRD-PARTY-TRANSITIVE-LICENSES.md` (transitive dependency table)

Evidence collection is delegated. Before rendering any markdown:

1. Read `.github/skills/dependency-license-evidence/SKILL.md` and follow it to produce or refresh the shared `tmp/` artifacts (`tmp/backend-packages.json`, `tmp/backend-package-licenses.json`, `tmp/frontend-transitive-licenses.csv`, `tmp/transitive-license-summary.json`).
2. Use only those `tmp/` artifacts as the source of truth for the markdown tables. Do not re-run inventory logic here; do not reshape evidence.
3. If the skill reports the run as blocked (for example the mandatory backend inventory command fails), STOP and report the run as blocked. Do not preserve previous markdown as if regeneration succeeded.

Markdown output rules (this workflow only):

1. Regenerate `THIRD-PARTY-LICENSES.md` and `THIRD-PARTY-TRANSITIVE-LICENSES.md` on every run so they stay aligned with the freshly produced `tmp/` artifacts.
2. Table row fields for every dependency (direct and transitive, frontend and backend):
   - project name
   - project homepage (resolved URL or `UNKNOWN`)
   - SPDX license identifier or expression (`UNKNOWN` only when the skill produced `UNKNOWN`)
   - license link (SPDX license page or resolved license URL, or `UNKNOWN`)
3. Keep frontend and backend clearly separated in tables. Split direct vs transitive across the two files.
4. Reflect the backend uniqueness rule enforced by the skill: a `(package, version)` marked as direct in `tmp/backend-package-licenses.json` MUST NOT reappear in the transitive markdown table.
5. Reflect internal / private packages separately from third-party rows when the skill flagged them as such. When the skill recorded an override in `tmp/transitive-license-summary.json` `overrides[]`, note the override explicitly next to the affected row (from → to, reason).
6. Blocked-license findings: surface every entry from `tmp/transitive-license-summary.json` `blocked[]` in the summary section with package, version, scope, ecosystem, and evidence source. Do not report a green status if `blocked[]` is non-empty.
7. Summary section MUST report:
   - counts (frontend direct, frontend transitive, backend direct, backend transitive, backend total)
   - NuGet resolution evidence breakdown from `nugetResolution`
   - unresolved counters
   - overrides applied
   - SPDX License List version from `spdxLicenseListVersion`
8. Execute the full markdown regeneration bundle in one run. Do not ask for confirmation between the two markdown files. Complete the bundle first, then report results.
9. After regeneration, perform a consistency pass so counts/status in `THIRD-PARTY-LICENSES.md` match the regenerated tables and the `tmp/transitive-license-summary.json` `counts` block.

Markdown-specific consistency checks (fail the run when any fails):

- `counts.backendDirect` MUST equal the number of backend direct rows in `THIRD-PARTY-LICENSES.md`.
- `counts.backendTransitive` MUST equal the number of backend rows in `THIRD-PARTY-TRANSITIVE-LICENSES.md`.
- `counts.frontendDirect` MUST equal the number of frontend direct rows in `THIRD-PARTY-LICENSES.md`.
- `counts.frontendTransitive` MUST equal the number of frontend rows in `THIRD-PARTY-TRANSITIVE-LICENSES.md`.
- `counts.backendTotal` MUST equal `counts.backendDirect + counts.backendTransitive` (already enforced by the skill; re-verify here).
- No markdown row contains raw placeholders like `LICENSE-FILE:<path>` or `LICENSE-URL:<url>` (already enforced by the skill; re-verify here).

## Output

After editing, provide:

1. Coverage achieved (which ecosystems/components are documented).
2. Whether the skill reused existing `tmp/` evidence or bootstrapped fresh evidence (state explicitly whether the run started from an empty `tmp/`). Include the exact backend inventory command executed and every `tmp/` artifact path produced.
3. Whether both markdown files were regenerated and are mutually consistent with `tmp/transitive-license-summary.json`.
4. Blocked-license findings with package, version, scope, ecosystem, and evidence source.
5. Open gaps requiring maintainer follow-up.
6. Unresolved / unknown license fields and any explicit override mappings applied.
7. NuGet license-resolution evidence summary from `nugetResolution` (registration-expression, nuspec-expression, nupkg-content-classification, url-content-classification, unresolved).

## Constraints

- Do not invent license names, versions, or attribution text.
- If evidence is missing at markdown rendering time, the skill has already marked it `UNKNOWN` — echo `UNKNOWN` in the row and do not guess.
- `THIRD-PARTY-LICENSES.md` and `THIRD-PARTY-TRANSITIVE-LICENSES.md` are required outputs, not optional appendices.
- Treat this as a one-shot workflow: partial completion is not acceptable when inputs are available.
- Avoid false green status when blocked-family variants are present under non-exact SPDX labels (already handled by the skill; do not override).
- Do not re-implement the evidence collection or NuGet resolution chain in this prompt; follow the skill.
- Do not rely on `build/` for temporary generated evidence; the skill uses `tmp/` for ephemeral files that should not be committed.
