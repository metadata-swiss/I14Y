---
agent: "agent"
description: "Create or update root CONTRIBUTING.md for the I14Y monorepo"
argument-hint: "Optional overrides: contribution workflow, branch strategy, commit convention, CLA/DCO requirements, execution mode (one-shot)"
---

## Role

You're a senior maintainer focused on contributor onboarding and repository governance.

## Task

Create or update a root `CONTRIBUTING.md` file that explains how to contribute safely and efficiently.

1. Inspect existing repository conventions before writing, including monorepo structure, build tooling, and contribution clues in docs or CI config.
2. Document the preferred workflow for:
   - reporting issues
   - proposing changes
   - opening pull requests
   - handling review feedback
3. Include technical contribution expectations relevant to this repository, including where to place backend vs frontend changes and expected validation steps.
4. Include a concise section on commit and branch naming only if repository conventions are clearly present.
5. If conventions are unclear, include a neutral default workflow and mark naming policy as maintainer-defined TODO.
6. Link to existing governance files if present (for example code of conduct, security policy, changelog, license).
7. Keep the content practical and short, with concrete steps and checklists where helpful.
8. Use natural, human wording in the document; avoid robotic labels such as "observed" or "evidence-based" in section titles and bullets.
9. Complete the contribution-guide update in one autonomous run when repository evidence is sufficient.
10. When documenting the default branch, refer to it as `main`.

## Output

After editing, provide:

1. A short summary of what was added or changed.
2. Any assumptions made where repository evidence was missing.
3. Follow-up items that may still need maintainer confirmation.

## Constraints

- Do not invent workflows that conflict with repository evidence.
- Prefer root-level `CONTRIBUTING.md` unless another location is already used.
- Keep contributor instructions actionable and non-redundant.
- Treat this as a one-shot workflow: do not stop after partial edits.
