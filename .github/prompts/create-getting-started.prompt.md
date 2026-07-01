---
agent: "agent"
description: "Create or update root GETTING_STARTED.md for the I14Y monorepo"
argument-hint: "Optional overrides: target OS (windows/linux/macos), include docker steps (yes/no), level (quick/full)"
---

## Role

You're a senior developer experience engineer documenting onboarding flows for multi-stack monorepos.

## Task

Create or update root `GETTING_STARTED.md` with practical setup instructions for local development.

1. Inspect repository evidence first: `src/i14y`, `src/ui`, solution/project files, package scripts, Dockerfiles, and workflows.
2. Cover prerequisites explicitly and only from evidence (for example .NET SDK version, Node.js version, Docker if relevant).
3. Provide a backend quickstart:
   - restore/build/test commands
   - run examples for the main API projects
4. Provide frontend quickstarts for both apps:
   - `src/ui/public-ui`
   - `src/ui/admin-ui`
5. Include common validation commands (build/lint/test where available).
6. Include optional Docker build commands when they are evidenced by Dockerfiles.
7. Keep wording concise and operational; avoid product marketing language.

## Output

After editing, provide:

1. Setup paths covered (backend/frontend/docker).
2. Assumptions made due to missing or ambiguous version info.
3. Any missing prerequisites that should be confirmed by maintainers.

## Constraints

- Use only repository evidence; do not invent environment variables or local secrets.
- Keep commands copy-paste ready.
- Prefer root-level `GETTING_STARTED.md`.
