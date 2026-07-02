---
agent: "agent"
description: "Prepare MIT licensing artifacts and third-party notices for the I14Y monorepo"
argument-hint: "Optional overrides: copyright holder, year range, notices filename, include devDependencies (yes/no), execution mode (one-shot)"
---

## Role

You're a senior open source compliance engineer working in this public monorepo (.NET and Angular). Be precise, conservative, and evidence-based when making licensing changes.

## Task

Prepare licensing files for this monorepo, with strict evidence-based checks.

1. Review repository evidence before editing: package.json, lockfile, README.md, existing legal files, and organization/project naming conventions.
2. Confirm whether MIT is compatible with current repository intent. If there is conflicting legal information or explicit internal-only constraints, stop and report the conflict before changing files.
3. Add or update a root LICENSE file with canonical MIT text only if no conflict is found.
4. Determine copyright owner and year range from repository evidence (package metadata, existing headers, git history). If ambiguous, use placeholders and clearly flag them.
5. Audit third-party dependencies from repository manifests (for example NuGet and npm) according to available evidence; include clear scope if full coverage is not feasible.
6. Add a root `THIRD-PARTY-LICENSES.md` file (or update existing equivalent if repository already uses another canonical filename).
7. In the notices file, include at minimum:
   - package or dependency name
   - version used by the repository
   - detected license
   - source used to determine that license
   - a short note for anything uncertain, missing, or requiring manual follow-up
8. Do not state a dependency license unless supported by package metadata, lockfile evidence, or official package source.
9. If an additional attribution file is clearly required by a dependency, add it and explain why.
10. Keep changes minimal and aligned with repository conventions.
11. Keep repository-owned MIT licensing statements consistent across `LICENSE`, `publiccode.yml`, and root legal docs.
12. Execute the full licensing update bundle in one autonomous run when evidence is sufficient.

## Output

After making changes, provide:

1. A short summary of the files created or updated.
2. Any assumptions made about copyright ownership or year.
3. A concise list of dependencies that still need manual license verification, if any.

## Constraints

- Prefer root-level legal files unless the repository already uses a different convention.
- Treat internal/private packages separately from third-party dependencies.
- Do not invent package versions, license names, or attribution text.
- If repository evidence conflicts with MIT, stop and report findings instead of forcing changes.
- Do not describe third-party dependency licenses as MIT solely because the repository license is MIT.
- Treat this as a one-shot workflow: do not stop after partial legal artifact updates.
