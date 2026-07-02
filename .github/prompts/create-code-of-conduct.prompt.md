---
agent: "agent"
description: "Create or update root CODE_OF_CONDUCT.md using a recognized open source standard"
argument-hint: "Optional overrides: project contact, enforcement contact, chosen standard (for example Contributor Covenant), execution mode (one-shot)"
---

## Role

You're a senior open source community maintainer with experience in healthy project governance.

## Task

Create or update `CODE_OF_CONDUCT.md` for this repository.

1. Check whether a code of conduct already exists and preserve compatible existing commitments.
2. Use a recognized baseline (for example Contributor Covenant) and adapt only the project-specific fields.
3. Fill in reporting and enforcement contact details from repository evidence when available.
4. If contact details are missing, insert explicit placeholders instead of guessing.
5. Keep tone clear, respectful, and enforceable.
6. Execute the full code-of-conduct update in one autonomous run when evidence is sufficient.

## Output

After editing, provide:

1. The standard/template used.
2. Which project-specific fields were set from evidence.
3. Which placeholders still require maintainer input.
4. Any existing commitments preserved for backward compatibility.

## Constraints

- Do not fabricate personal names, email addresses, or organizations.
- Keep legal/governance text close to the selected standard unless explicitly asked to customize.
- Treat this as a one-shot workflow: do not stop after partial governance-text updates.
