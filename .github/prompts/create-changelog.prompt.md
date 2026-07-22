---
agent: "agent"
description: "Create or update root monorepo CHANGELOG.md in Keep a Changelog format"
argument-hint: "Optional overrides: start year (default current year), include older history, aggregation mode (minor|patch), collapse sections, execution mode (one-shot)"
---

## Role

You're a release engineering expert focused on transparent, user-facing change history.

## Task

Create or update `CHANGELOG.md` for this repository.

1. Use a Keep a Changelog-compatible structure, but omit `Unreleased` when there is no unreleased content.
2. Build release entries from repository evidence (Git tags and commit messages), not guesses.
3. By default, focus on recent history only: start with the current year and expand to older years only if explicitly requested.
4. By default, aggregate by minor releases using this repository convention: release `x.y` summarizes its own minor stream `x.y.z`.
5. Determine whether a release is published before creating a dated release section.
6. Accepted evidence for a published release (in priority order):
	- GitHub Release publication metadata for the tag
	- Annotated tag metadata (`taggerdate`)
	- explicit maintainer confirmation in the request
7. Lightweight tags pointing to commits are not sufficient proof of a published release date on their own.
8. If only lightweight tags are available and no release publication metadata is accessible, keep the stream under `Unreleased` (or `Planned`) and explicitly report this limitation.
9. If a dated release section is created from tag metadata, state the evidence source in the output.
10. If explicitly requested, switch to patch-level listing; otherwise do not create one section per patch tag when tags are too granular.
11. Include only released minor streams by default; exclude the current in-progress minor stream unless explicitly requested.
12. For each included release, summarize user-facing changes across relevant monorepo components from the underlying stream tags/commits in the computed scope.
13. Inspect commit subjects/bodies in each included range for story/work-item references (for example `#1234`, `AB#1234`, `US-1234`, `PROJ-1234`, `owner/repo#1234`).
14. When story/work-item links are publicly accessible, fetch each referenced story title and short description/state, then use that context to improve changelog wording.
15. If a tracker is inaccessible or a reference cannot be resolved, keep the commit-derived summary and explicitly report unresolved references in the output assumptions.
16. Organize changes under clear categories (Added, Changed, Fixed, Removed, Security, Deprecated).
17. Exclude low-value internal-only entries that are not user-facing (for example versioning-tool-only updates such as GitVersion-only commits, next-version bumps, release branch merge noise), unless explicitly requested.
18. If reliable history is not available, create a clean baseline changelog and clearly mark what could not be inferred.
19. Keep entries concise but useful; prefer many accurate entries over a small generic summary.
20. Do not add generic intro boilerplate about Keep a Changelog or Semantic Versioning unless explicitly requested.
21. Normalize encoding artifacts from imported commit subjects: remove mojibake or unsupported symbols and output clean ASCII-safe text.
22. If scope has mixed backend/frontend changes, keep entries grouped by user impact, not by internal folder names.
23. Execute changelog regeneration in one autonomous run: gather evidence, update file, and self-check consistency before reporting.

## Output

After editing, provide:

1. Whether historical entries were added or not.
2. Sources used for history (tags, commit ranges, release metadata).
3. Aggregation strategy used (minor or patch) and exact stream mapping used for each section (for example `1.4 <- 1.4.z`).
4. Which years/releases were intentionally included or excluded.
5. Release-date evidence used for each dated section (GitHub Release metadata, annotated tag metadata, or maintainer confirmation).
6. Story/work-item enrichment coverage: detected reference format(s), number of references resolved/unresolved, and inaccessible trackers.
7. Any assumptions or missing release metadata.
8. Any excluded non-user-facing entries and why they were excluded.

## Constraints

- Do not invent version numbers or dates.
- Prefer accuracy over completeness when source data is ambiguous.
- Do not keep placeholder lines that provide no user value (for example, "Release tag detected..." or purely tooling-only notes) when better evidence exists.
- Avoid empty sections and filler text.
- Do not keep mojibake sequences (for example `ΓÇª`, `≡ƒöÑ`) in the final file.
- Treat this as a one-shot workflow: do not stop after partial release coverage.
