---
name: i14y-review
description: Review the current I14Y branch or a given PR/commit range against the team's review rules (correctness first, smallest fix, no scope creep, C# and TypeScript type safety, API compatibility). Use when asked to review a PR, a branch, a diff, or AI-generated changes in this repository.
---

# I14Y code review

## 1. Get the diff
- **No argument:** review `git diff main...HEAD` plus any uncommitted changes (`git diff HEAD`).
- **A PR number:** use `gh pr diff <n>` if `gh` is installed. Otherwise run `git fetch origin pull/<n>/head:pr-<n>`, then `git diff main...pr-<n>`.
- **A commit range:** use `git diff <range>`.

Read the stated purpose (PR title and body, or the commit messages). The purpose defines the scope.

**Measure the size** of the hand-written change, using the same range as the diff:
```bash
git diff --shortstat <range> -- . ':(exclude)**/Generated/**' ':(exclude)build/ts-client/**' ':(exclude)**/api-client/lib/generated/**' ':(exclude)**/Migrations/**' ':(exclude)**/package-lock.json' ':(exclude)**/i18n/*.json'
```
Report the line count (insertions + deletions) at the top of the review.
- **Over about 400 lines:** name concrete split points, e.g. by layer (Data + migration → Core → Admin + clients → UI) or refactor vs feature.
- **Over about 1,000 lines:** this is a finding in its own right, unless the PR says it is a refactor or a removal.
- **A refactor mixed with a feature:** always a finding, whatever the size.

## 2. Review in this priority order
1. Data corruption or data loss. Watch EF migrations, deletes, and triple-store writes.
2. Incorrect business logic.
3. Type mismatches that break compilation or fail at runtime (C# and `.ts`).
4. Breaking API behaviour: request and response models, JSON names, nullability, status codes, error shape, generated clients.
5. Security: authorization on new endpoints, secrets, injection into SPARQL or Elasticsearch queries.
6. Performance: N+1 queries, a missing `AsNoTracking`, unbounded queries.
7. Concurrency: channel dispatchers, `BackgroundService`, shared state.
8. Maintainability.
9. Style. Report style **only** if nothing above it was found.

## 3. I14Y-specific checks
- Write handlers in Core must notify both `IAuditTrailNotifierService` and `ISearchIndexNotifierService`.
- `cancellationToken` must be passed all the way down. No `.Result`/`.Wait()`. No scoped service captured by a singleton.
- No hand edits under `*/Generated/`, `build/ts-client/generated` or `api-client/lib/generated`. If a contract changed, the clients must be regenerated.
- A Core contract change must have matching updates in the Admin, Partner and Iri callers and in the TypeScript client.
- `.ts` changes must not use `any`, `as` or `!`, and must not change tsconfig to hide a mismatch. Form value types must match the models they fill.
- Translation keys added in code must exist in every language file.
- Code must follow `.claude/rules/csharp-conventions.md`. Undecided conventions follow the surrounding file and are **not** findings.

## 4. Scope control
Flag any of these when they fall outside the stated purpose:
- unrelated refactors, renames, reformatting or error-handling changes,
- dependency upgrades,
- new abstractions (interfaces, factories, helpers, generic repositories, config options) without a concrete current need,
- generalisation for hypothetical future use.

AI-generated changes deserve extra attention to scope creep and to type-safety regressions.

## 5. Report
Report only findings that have a concrete technical reason. Give each finding:
- `file:line`
- **What** is wrong and **why** it is a problem
- **Impact**: what breaks, for whom, and when
- **Smallest fix**

For type findings, also give the two incompatible types, and say whether the problem fails at compile time or is a runtime risk.

**Never** write vague findings ("could be cleaner", "consider refactoring") or preference-only findings.

End with the two final questions, each answered yes or no with a reason:
- Does this solve the stated problem with the smallest reasonable change while preserving behaviour elsewhere?
- Are all values, parameters, return types, API models, form values and observables type-compatible without unsafe casts?
