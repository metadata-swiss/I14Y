---
agent: "agent"
description: "Create or update root THIRD-PARTY-LICENSES.md for the I14Y monorepo"
argument-hint: "Optional overrides: include dev dependencies (yes/no), include NuGet coverage (yes/no), table format"
---

## Role

You're a software compliance engineer focused on evidence-based third-party license documentation.

## Task

Create or update root `THIRD-PARTY-LICENSES.md` for this monorepo.

1. Inspect repository manifests and existing notices before writing.
2. Use existing repository evidence to define current coverage (for example existing notices under UI subprojects).
3. Document what is covered and what is pending (for example backend NuGet consolidation), without guessing.
4. For each dependency entry included, provide evidence source (manifest, lockfile, package metadata, or existing notice file).
5. Keep internal/private packages clearly separated when license metadata is incomplete.
6. Keep the file concise and auditable.

## Output

After editing, provide:

1. Coverage achieved (which ecosystems/components are documented).
2. Open gaps requiring maintainer follow-up.
3. Any unresolved or unknown license fields.

## Constraints

- Do not invent license names, versions, or attribution text.
- If evidence is missing, mark it explicitly as TODO or unknown.
- Prefer root-level `THIRD-PARTY-LICENSES.md`.
