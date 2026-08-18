---
agent: "agent"
description: "Create or update root monorepo CHANGELOG.md in Keep a Changelog format"
argument-hint: "Optional overrides: start year (default current year), include older history, aggregation mode (minor|patch), collapse sections, execution mode (one-shot)"
---

## Role

You're a release engineering expert focused on transparent, user-facing change history.

## Task

Create or update `CHANGELOG.md` for this repository.

### Mandatory Reliability Gates (must pass before editing)

If any gate fails, stop and ask for maintainer confirmation instead of guessing.

Gate A - Release mapping lock:

- Derive and print an explicit mapping table before writing, for example:
  - `2.2.0 <- 2.1.y (y > 0)`
  - `2.1.0 <- 2.0.y (y > 0)`
- Do not proceed until each release section maps to exactly one source stream.

Gate B - Date evidence lock:

- Every dated release section must have one evidence source: GitHub release metadata, annotated tag metadata, or repository release-cut evidence under this repository convention.
- If evidence is missing, do not create a dated section.

Gate C - Content relevance lock:

- Every bullet must be user-facing and traceable to commits/tags in the mapped stream.
- Remove policy/process narration from changelog bullets.

Gate D - Duplicate/conflict lock:

- The same user-facing change must appear in exactly one release section.
- If a change appears in multiple candidate sections, keep it only in the section selected by mapping.

Gate E - Human-quality lock:

- Bullets must describe product outcomes (what changed for users), not implementation steps (how it was built).
- Collapse noisy commit series into one semantic bullet when they describe the same outcome.

1. Use a Keep a Changelog-compatible structure, but omit `Unreleased` when there is no unreleased content.
2. Build release entries from repository evidence (Git tags and commit messages), not guesses.
3. By default, focus on recent history only: start with the current year and expand to older years only if explicitly requested.
4. By default, aggregate by release tags ending in `.0` using this repository convention: release `M.N.0` summarizes the previous minor stream `M.(N-1).z` for all `z > 0`.
5. For this repository's current line (`2.x`), apply the rule explicitly as: release `2.x.0` summarizes `2.(x-1).y` for all `y > 0`.
6. User-facing change bullets come from the mapped previous-minor stream, not from policy text.
   6a. Repository date convention: if weekly release policy defines a fixed release day (for example Wednesday), use the release-cut date on that day from the mapped previous-minor stream; if the `.0` lightweight tag timestamp differs, treat it as technical tagging time and keep the release-cut date.
7. Determine whether a release is published before creating a dated release section.
8. Accepted evidence for a published release (in priority order):
   - GitHub Release publication metadata for the tag
   - Annotated tag metadata (`taggerdate`)
   - Release-cut date from the mapped previous-minor stream when explicitly confirmed by maintainer/release policy
   - Lightweight release-tag target commit date (only when no stronger policy evidence exists)
   - explicit maintainer confirmation in the request
9. If evidence sources conflict, apply this tie-breaker order: GitHub Release metadata > annotated tag metadata > maintainer-confirmed release policy/cut date > lightweight tag target commit date.
10. If release-date evidence is unavailable or still ambiguous after tie-breakers, keep the stream under `Unreleased` (or `Planned`) and explicitly report this limitation.
11. If a dated release section is created from tag metadata, state the evidence source in the output.
12. If explicitly requested, switch to patch-level listing; otherwise do not create one section per patch tag when tags are too granular.
13. Include only released minor streams by default; exclude the current in-progress minor stream unless explicitly requested.
14. For each included release, summarize user-facing changes across relevant monorepo components from the underlying stream tags/commits in the computed scope.
15. Reconstruct release notes at human quality from commit/tag evidence by clustering related commits into higher-level product changes (feature, UX improvement, API capability, bugfix), instead of listing raw commit messages.
16. Target the same abstraction level as human release notes: describe user-visible outcomes and capabilities, not internal implementation steps.
17. Build a thematic summary for each release using product-facing buckets when applicable (for example Structures, Data model, UX improvements, Partner API, Bugfixes, Security).
18. Prefer semantic aggregation: collapse repetitive technical commits (multiple bumps/fixes for one outcome) into one clear user-facing bullet when they represent the same product impact.
19. If maintainers provide human release notes as examples, treat them as the expected quality/style benchmark and verify that generated themes and highlights are equivalent in scope.
20. Do not introduce a benchmark-only highlight if no supporting commit/tag evidence exists in the computed scope.
21. Coverage-first rule: do not over-compress distinct user-facing outcomes into a single generic bullet.
22. Keep one bullet per distinct product outcome when impacts differ (for example permission change, new export format, filter behavior, partner API capability, UI workflow fix).
23. For each released section, aim for breadth comparable to human release notes: include all major evidenced themes rather than only a minimal summary.
24. Inspect commit subjects/bodies in each included range for story/work-item references (for example `#1234`, `AB#1234`, `US-1234`, `PROJ-1234`, `owner/repo#1234`).
25. When story/work-item links are publicly accessible, fetch each referenced story title and short description/state, then use that context to improve changelog wording.
26. If a tracker is inaccessible or a reference cannot be resolved, keep the commit-derived summary and explicitly report unresolved references in the output assumptions.
27. Organize changes under clear categories (Added, Changed, Fixed, Removed, Security, Deprecated).
28. Exclude low-value internal-only entries that are not user-facing (for example versioning-tool-only updates such as GitVersion-only commits, next-version bumps, release branch merge noise), unless explicitly requested.
29. If reliable history is not available, create a clean baseline changelog and clearly mark what could not be inferred.
30. Keep entries concise but useful; prefer many accurate entries over a small generic summary.
31. Do not add generic intro boilerplate about Keep a Changelog or Semantic Versioning unless explicitly requested.
32. Normalize encoding artifacts from imported commit subjects: remove mojibake or unsupported symbols and output clean ASCII-safe text.
33. If scope has mixed backend/frontend changes, keep entries grouped by user impact, not by internal folder names.
34. Execute changelog regeneration in one autonomous run: gather evidence, update file, and self-check consistency before reporting.
35. Do not include process or policy statements inside release bullets (for example mapping-rule explanations); keep such notes only in the post-edit report.
36. Before finalizing, run a strict self-check and fail if any check fails:
    - release order is reverse chronological;
    - no policy/mapping/process bullet exists;
    - no duplicated bullet text across release sections;
    - each dated section has evidence type explicitly identified in the report;
    - each section has at least one meaningful user-facing bullet;
    - each released section contains multiple distinct outcome bullets unless evidence is genuinely sparse (in that case, explicitly report the sparsity reason).

## Output

After editing, provide:

1. Whether historical entries were added or not.
2. Sources used for history (tags, commit ranges, release metadata).
3. Aggregation strategy used (minor or patch) and exact stream mapping used for each section (for example `1.4 <- 1.4.z`).
4. Which years/releases were intentionally included or excluded.
5. Release-date evidence used for each dated section (GitHub Release metadata, annotated tag metadata, maintainer-confirmed release-cut date, lightweight release-tag target commit date, or maintainer confirmation).
6. Story/work-item enrichment coverage: detected reference format(s), number of references resolved/unresolved, and inaccessible trackers.
7. Any assumptions or missing release metadata.
8. Any excluded non-user-facing entries and why they were excluded.
9. If benchmark release notes were provided, a short coverage comparison: matched themes, missing themes, and excluded benchmark items with commit-evidence reason.
10. A brief quality check on abstraction level: confirm that generated bullets are outcome-oriented and comparable to human release note style.
11. Coverage check per release: list detected user-facing themes and confirm none were collapsed into a generic catch-all bullet.
12. Reliability-gate status: pass/fail for Gate A to Gate E, with one-line justification per gate.

## Constraints

- Do not invent version numbers or dates.
- Prefer accuracy over completeness when source data is ambiguous.
- Do not keep placeholder lines that provide no user value (for example, "Release tag detected..." or purely tooling-only notes) when better evidence exists.
- Do not include mapping-policy narration as changelog bullet content.
- Avoid empty sections and filler text.
- Do not keep mojibake sequences (for example `ΓÇª`, `≡ƒöÑ`) in the final file.
- Treat this as a one-shot workflow: do not stop after partial release coverage.
