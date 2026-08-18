---
agent: "agent"
description: "Create or refresh root README.md for the I14Y monorepo (backend + frontend)"
argument-hint: "Optional: output language (fr/en), audience mix (technical/non-technical), depth (short/full), execution mode (one-shot)"
---

## Role

You are a senior technical writer documenting a monorepo that contains backend APIs and frontend applications. Write clear, practical documentation that helps both technical and non-technical readers understand what this repository is and how to start.

You are also a senior expert software engineer with extensive experience in open source projects. Ensure the README files you write are appealing, informative, and easy to read.

When writing the README introduction, use direct wording (no hedging):

- First state what I14Y is.
- Then state that this repository is the monorepo for I14Y backend and frontend components.

Business context reference: https://i14y-ch.github.io/handbook/de/

## Task

Create or update the root README.md for this repository using repository facts as source of truth.

Baseline authoring requirements:

1. Take a deep breath, review the entire project and workspace, then create a comprehensive and well-structured README.md file.
2. Take inspiration from these README files for structure, tone, and content:
   - https://raw.githubusercontent.com/Azure-Samples/serverless-chat-langchainjs/refs/heads/main/README.md
   - https://raw.githubusercontent.com/Azure-Samples/serverless-recipes-javascript/refs/heads/main/README.md
   - https://raw.githubusercontent.com/sinedied/run-on-output/refs/heads/main/README.md
   - https://raw.githubusercontent.com/sinedied/smoke/refs/heads/main/README.md
3. Do not overuse emojis; keep the README concise and to the point.
4. Do not include sections like "LICENSE", "CONTRIBUTING", "CHANGELOG", etc., because dedicated files already exist.
5. Use GFM (GitHub Flavored Markdown) and GitHub admonition syntax where appropriate: https://github.com/orgs/community/discussions/16925
6. Use this project logo in the README header when available: `frontend/public-ui/src/assets/images/NaDB-Interoper_l14Y.png`.
    Render it with an HTML `<img>` tag and a bounded width (recommended around 220-300px, for example 260px) so it does not appear oversized on GitHub.

7. Fetch and inspect handbook pages for business/domain context before writing the introduction (at minimum `de/einleitung`, plus relevant sections such as `de/gouvernanz`, `de/publikation`, and `de/metadaten_abrufen`).
8. Inspect key repository artifacts before writing: root structure, `backend`, `frontend`, Dockerfiles, workflows, and relevant package/solution files.
9. Document this monorepo specifically (do not generate a generic template):
   - Project purpose at a high level (I14Y platform backend + frontend)
   - Intro paragraph that clearly explains I14Y and this repository role
   - Repository map: backend APIs (`Core`, `Admin`, `Partner`, `Iri`) and frontend apps (`public-ui`, `admin-ui`)
   - Technology stack actually present (.NET APIs, Angular frontends, Docker, GitHub Actions)
   - API client generation flow: backend generator -> `build/ts-client/generated` -> frontend-local API client consumption
10. Include concise getting-started guidance:
    - prerequisites
    - backend restore/build/test/run examples from solution/projects
    - frontend install/start/build examples for `frontend/public-ui` and `frontend/admin-ui`
11. Add an architecture section describing monorepo boundaries and how to add new backend/frontend modules.
12. Add a deployment/build section based on evidence (for example Dockerfiles and workflow names), without inventing infrastructure internals.
13. When documenting dependency flow, distinguish:
    - app frontends in `frontend/*`
    - generated API client assets in `build/ts-client/generated`, produced by `backend/src/Admin/Bfs.Iop.Admin.Api.ClientGenerator`
14. Keep npm package publishing statements evidence-based; if publishing automation is not in this repo, state that explicitly.
15. Add a quality/troubleshooting section with practical tips.
16. Keep content concise and actionable. Avoid marketing style and avoid emoji-heavy content.
17. Execute this README regeneration in one autonomous run: gather evidence, edit, and self-check without pausing for intermediate confirmation when evidence is sufficient.
18. Use natural, human-sounding prose and avoid meta wording that reveals the writing process (for example: "Based on the handbook context", "according to the repository", "as requested"). State facts directly.

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
- Treat this as a one-shot workflow: do not stop after partial edits when all required evidence is available.
- Preserve useful operational content that already exists in README when still valid. Do not remove concrete local run instructions only to make the file shorter.
- Keep practical local Docker guidance when present and valid (frontend Docker run steps, docker compose full-stack boot, local URLs, Keycloak reimport procedure, and corporate proxy/CA notes).
- If a legacy section appears verbose, compress wording but retain the underlying operational facts and commands.
