---
agent: "agent"
description: "Create or update root CONTRIBUTING.md for the I14Y monorepo"
argument-hint: "Optional overrides: contribution workflow, branch strategy, commit convention, CLA/DCO requirements, execution mode (one-shot)"
---

## Role

You're a senior maintainer focused on contributor onboarding and repository governance.

## Task

Create or update a root `CONTRIBUTING.md` file that clearly states external code contributions are not accepted at this time.

1. Inspect existing repository conventions before writing, including monorepo structure, build tooling, and contribution clues in docs or CI config.
2. Document the preferred workflow for reporting ideas and requests by opening an issue in `https://github.com/I14Y-ch/feature-requests`.
3. Explicitly state that pull requests and direct external code contributions are currently not accepted.
4. Do not include implementation-level contribution steps (branch strategy, commit convention, test checklist) unless they are strictly internal and clearly marked as maintainer-only.
5. If conventions are unclear, keep the message minimal and avoid suggesting a default external contribution workflow.
6. Link to existing governance files if present (for example code of conduct, security policy, changelog, license).
7. Keep the content practical and short, with concrete steps for issue submission where helpful.
8. Use natural, human wording in the document; avoid robotic labels such as "observed" or "evidence-based" in section titles and bullets.
9. Complete the contribution-guide update in one autonomous run when repository evidence is sufficient.
10. Do not encourage forks, pull requests, or external branch workflows.

## Output

After editing, provide:

1. A short summary of what was added or changed.
2. Any assumptions made where repository evidence was missing.
3. Follow-up items that may still need maintainer confirmation, especially around when or whether external contributions may open in the future.

## Constraints

- Do not invent workflows that conflict with repository evidence.
- Prefer root-level `CONTRIBUTING.md` unless another location is already used.
- Keep contributor instructions actionable and non-redundant.
- Ensure the final `CONTRIBUTING.md` includes the exact issue URL `https://github.com/I14Y-ch/feature-requests`.
- Treat this as a one-shot workflow: do not stop after partial edits.
