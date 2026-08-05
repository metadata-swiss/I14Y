---
agent: "agent"
description: "Create or update root THIRD-PARTY-LICENSES.md for the I14Y monorepo"
argument-hint: "Optional overrides: include dev dependencies (yes/no), include NuGet coverage (yes/no), blocked-license regex mode (strict/family), internal-license override mapping, execution mode (one-shot)"
---

## Role

You're a software compliance engineer focused on evidence-based third-party license documentation.

## Task

Create or update all third-party licensing artifacts for this monorepo:

- root `THIRD-PARTY-LICENSES.md` (summary + policy status + direct dependency tables)
- root `THIRD-PARTY-TRANSITIVE-LICENSES.md` (transitive dependency table)
- `tmp/frontend-transitive-licenses.csv` (frontend export, temporary/non-versioned)
- `tmp/transitive-license-summary.json` (machine-readable summary, temporary/non-versioned)

1. Inspect repository manifests and existing notices before writing.
2. Frontend scope must include all current lockfiles:
   - `frontend/public-ui/package-lock.json`
   - `frontend/admin-ui/package-lock.json`
3. Produce evidence-backed direct and transitive inventories for npm and keep backend NuGet coverage clearly separated.
4. Regenerate `THIRD-PARTY-LICENSES.md` and `THIRD-PARTY-TRANSITIVE-LICENSES.md` on every run so they stay aligned with lockfiles/manifests.
5. Backend inventory execution is mandatory on every run: execute `dotnet list <solution-or-project> package --include-transitive --format json` and persist the raw output under `tmp/`.
6. If backend inventory execution fails, stop and report the run as blocked; do not silently preserve old backend sections as if regeneration succeeded.
7. Regenerate backend sections in detailed artifacts from that backend inventory output.
8. For backend NuGet packages, resolve license evidence per package+version from official NuGet metadata (registration API and package nuspec) instead of relying on `dotnet list` fields alone.
9. Prefer SPDX/license expression when available.
10. If nuspec or registration points to `license` type `file`, download the corresponding `.nupkg`, read the referenced license file content, and classify to a SPDX identifier using deterministic text fingerprints (for example MIT / Apache-2.0 / BSD-3-Clause / RPL-1.5).
11. If only a license URL exists, fetch the license text and classify to SPDX using the same deterministic fingerprints.
12. Do not keep raw placeholders like `LICENSE-FILE:<path>` or `LICENSE-URL:<url>` in final markdown tables; keep SPDX when classification succeeds.
13. Mark backend package license as `UNKNOWN` only when metadata lookup or content classification fails.
14. Apply blocked-license checks to direct + transitive, runtime + dev scopes.
15. For blocked checks, do not rely only on exact SPDX strings; also detect family variants (for example `LGPL-3.0-only`, `LGPL-3.0-or-later`).
16. Keep internal/private packages clearly separated when license metadata is incomplete.
17. If an internal override policy is requested (for example `UNKNOWN -> MIT` for a private package), keep explicit traceability notes in summary output.
18. Document what is covered and what is pending, without guessing.
19. Keep files concise and auditable.
20. Execute the full regeneration bundle in one run (summary + direct + transitive + tmp artifacts) and do not stop after partial updates.
21. Do not ask for confirmation between files when evidence is sufficient; complete the full bundle first, then report results.
22. After regeneration, perform a consistency pass so counts/status in `THIRD-PARTY-LICENSES.md` match the regenerated tables.
23. Enforce backend uniqueness across direct and transitive tables by package+version: if the same package+version appears in both, keep it only in direct and remove it from transitive.
24. For each dependency row in direct and transitive tables, include: project name, project homepage, SPDX license identifier, and a license link.
25. Keep links auditable: use resolved URLs when available; otherwise set field value to `UNKNOWN`.
26. Validate consistency directly in the workflow and fail the run when any check fails:

- backend package+version overlap count between direct and transitive must be `0`;
- summary backend direct count must equal backend direct rows in `THIRD-PARTY-LICENSES.md`;
- summary backend transitive count must equal backend rows in `THIRD-PARTY-TRANSITIVE-LICENSES.md`;
- summary backend total must equal direct + transitive.

## Output

After editing, provide:

1. Coverage achieved (which ecosystems/components are documented).
2. Whether all target files were regenerated and are mutually consistent.
3. Whether tmp artifacts were regenerated (or explicitly skipped because absent).
4. Whether backend sections in detailed artifacts were regenerated from a fresh backend inventory run, including the exact command executed and the `tmp/` artifact path.
5. Blocked-license findings with package, version, scope, and evidence path.
6. Open gaps requiring maintainer follow-up.
7. Any unresolved or unknown license fields and any explicit override mappings applied.
8. NuGet license-resolution evidence summary (how many resolved via registration expression, nuspec expression, nupkg content classification, URL content classification, unresolved).

## Constraints

- Do not invent license names, versions, or attribution text.
- If evidence is missing, mark it explicitly as TODO or unknown.
- Prefer root-level `THIRD-PARTY-LICENSES.md`.
- `THIRD-PARTY-LICENSES.md` and `THIRD-PARTY-TRANSITIVE-LICENSES.md` are required outputs, not optional appendices.
- Treat this as a one-shot workflow: partial completion is not acceptable when inputs are available.
- Avoid false green status when blocked-family variants are present under non-exact SPDX labels.
- NuGet backend licensing must be evidence-backed at package+version level (registration/nuspec/nupkg/URL content); do not infer license from package family names.
- Raw placeholders such as `LICENSE-FILE:<path>` or `LICENSE-URL:<url>` are not acceptable final license values in markdown tables when SPDX classification is possible.
- Do not rely on `build/` for temporary generated evidence; use `tmp/` for ephemeral files that should not be committed.
