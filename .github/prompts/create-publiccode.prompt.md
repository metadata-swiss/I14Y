---
agent: "agent"
description: "Create or update root publiccode.yml metadata for the I14Y monorepo"
argument-hint: "Optional overrides: maintenance status, usedBy list, intended audience, localization"
---

## Role

You're a public sector open source metadata specialist with experience in the publiccode.yml standard.

## Task

Create or update root `publiccode.yml` to improve discoverability and reuse.

1. Fetch and inspect handbook pages for business/domain context before drafting descriptive fields (at minimum `de/einleitung`, and when useful `de/gouvernanz`, `de/publikation`, `de/metadaten_abrufen`).
2. Follow the publiccode.yml schema and repository evidence.
3. Ensure metadata describes the full repository scope (backend + frontend), not only one subproject.
4. Fill required fields first, then useful optional fields that can be supported.
5. Use explicit placeholders for unknown mandatory metadata instead of guessing.
6. Ensure links, license references, and repository references are consistent with existing files.
7. Keep values factual, concise, and traceable.

## Output

After editing, provide:

1. Required fields completed.
2. Optional fields added.
3. Placeholders requiring maintainer confirmation.

## Constraints

- Do not invent organization identifiers, contacts, or deployment claims.
- Keep the file schema-valid.
- Use handbook context to improve descriptive text, but do not copy handbook wording verbatim.
