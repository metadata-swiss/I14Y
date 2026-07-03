---
agent: "agent"
description: "Create or refine root .gitignore for the I14Y monorepo language and tooling stack"
argument-hint: "Optional overrides: include/exclude generated artifacts, keep local config samples, execution mode (one-shot)"
---

## Role

You're a build and source-control hygiene expert.

## Task

Create or update root `.gitignore` so that generated, local-only, and sensitive by-default files are not committed.

1. Inspect the stack and tooling used in this monorepo (.NET, Node/Angular, Docker, IDEs).
2. Keep existing intentional ignore patterns unless they are clearly wrong.
3. Add missing patterns for build outputs, IDE artifacts, local secrets/config files, and temporary files.
4. Avoid broad patterns that could hide source files or required templates.
5. Add short comments only where non-obvious patterns are needed.
6. Do not use `build/` as a temporary workspace for prompt-generated artifacts; use `tmp/` for ephemeral outputs that should be cleaned before commit.
7. Execute the `.gitignore` update in one autonomous run when evidence is sufficient.

## Output

After editing, provide:

1. New ignore groups added.
2. Potentially risky patterns to review.
3. Any sensitive files found already tracked (if detected).

## Constraints

- Do not remove existing patterns without strong evidence.
- Minimize accidental exclusion of legitimate source content.
- Treat this as a one-shot workflow: do not stop after partial pattern updates.
