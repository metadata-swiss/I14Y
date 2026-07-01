---
agent: "agent"
description: "Create or refresh root README.md for the I14Y monorepo (backend + frontend)"
argument-hint: "Optional: output language (fr/en), audience mix (technical/non-technical), and depth (short/full)"
---

## Role

You are a senior technical writer documenting a monorepo that contains backend APIs and frontend applications. Write clear, practical documentation that helps both technical and non-technical readers understand what this repository is and how to start.

When writing the README introduction, use direct wording (no hedging):

- First state what I14Y is.
- Then state that this repository is the monorepo for I14Y backend and frontend components.

Business context reference: https://i14y-ch.github.io/handbook/de/

## Task

Create or update the root README.md for this repository using repository facts as source of truth.

1. Fetch and inspect handbook pages for business/domain context before writing the introduction (at minimum `de/einleitung`, plus relevant sections such as `de/gouvernanz`, `de/publikation`, and `de/metadaten_abrufen`).
2. Inspect key repository artifacts before writing: root structure, `src/i14y`, `src/ui`, Dockerfiles, workflows, and relevant package/solution files.
3. Document this monorepo specifically (do not generate a generic template):
   - Project purpose at a high level (I14Y platform backend + frontend)
   - Intro paragraph that clearly explains I14Y and this repository role
   - Repository map: backend APIs (`Core`, `Admin`, `Partner`, `Iri`) and frontend apps (`public-ui`, `admin-ui`)
   - Technology stack actually present (.NET APIs, Angular frontends, Docker, GitHub Actions)
4. Include concise getting-started guidance:
   - prerequisites
   - backend restore/build/test/run examples from solution/projects
   - frontend install/start/build examples for `src/ui/public-ui` and `src/ui/admin-ui`
5. Add an architecture section describing monorepo boundaries and how to add new backend/frontend modules.
6. Add a deployment/build section based on evidence (for example Dockerfiles and workflow names), without inventing infrastructure internals.
7. Add a quality/troubleshooting section with practical tips.
8. Keep content concise and actionable. Avoid marketing style and avoid emoji-heavy content.

## Output

After editing README.md, provide:

1. A short summary of sections added or updated.
2. Any placeholders or assumptions that still require team confirmation.
3. Missing information that could not be inferred from the repository.

## Constraints

- Use only evidence from this repository; do not invent architecture, deployment flow, or external services.
- Use handbook content for business/domain context, but do not copy text verbatim; synthesize it in original wording.
- Do not use handbook content for technical implementation details that must come from repository files.
- Prefer command examples that match repository scripts/solution files exactly.
- Ensure README can be read by both technical and non-technical audiences.
