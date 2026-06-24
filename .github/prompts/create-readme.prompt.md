---
agent: "agent"
description: "Create or refresh README.md for BFS IOP Public UI (Angular)"
argument-hint: "Optional: output language (fr/en), audience (dev/public), and depth (short/full)"
---

## Role

You are a senior frontend engineer documenting Angular public-facing platforms. Write clear, practical documentation that helps a new developer run and maintain this project quickly.

When writing the README introduction, use direct wording (no hedging):

- First state what I14Y is.
- Then state that this repository is the Angular public frontend for browsing and exploring the metadata catalog.

Business context reference: https://i14y-ch.github.io/handbook/de/

## Task

Create or update the root README.md for this repository using repository facts as source of truth.

1. Inspect key project files before writing: package.json, angular.json, src/app structure, and environment/config folders.
2. Document this project specifically (do not generate a generic Angular template):
   - Project purpose at a high level (I14Y metadata catalog public frontend)
   - Intro paragraph that clearly explains I14Y and this frontend's role
   - Main functional areas from src/app (home, catalog, concepts, datasets, data-services, mappingtables, organisations, public-services, metasearch, news)
   - Stack and key libraries actually present (Angular, Angular Material, Oblique, ngx-translate, angular-oauth2-oidc, ngx-matomo-client, ESLint, Prettier)
3. Include concise setup and usage sections based on existing npm scripts:
   - install, start, build, build-prod, lint, lint:fix, prettier, audit, format
   - If a commonly used local startup command exists in repository context/discussion (for example custom host/port or polling), include it as an optional variant with short guidance.
4. Add an architecture section that explains folder conventions and where to add new features.
5. Add a configuration section that explains environments and static config locations without exposing secrets (for example environments/\*.ts, src/assets/config, staticwebapp.config.json).
6. Add a quality section (lint/format/audit) and a troubleshooting section with practical tips.
7. Keep content concise and actionable. Avoid marketing style and avoid emoji-heavy content.

## Output

After editing README.md, provide:

1. A short summary of sections added or updated.
2. Any placeholders or assumptions that still require team confirmation.
3. Missing information that could not be inferred from the repository.

## Constraints

- Use only evidence from this repository; do not invent architecture, deployment flow, or external services.
- Use the handbook link only for business/domain context (what I14Y is), not for technical details that must come from repository files.
- Prefer command examples that match package.json scripts exactly.
- Keep README focused on developer onboarding and daily workflow.
- Do not add LICENSE/CONTRIBUTING/CHANGELOG sections unless already present in repository conventions.
