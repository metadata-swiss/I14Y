---
agent: "agent"
description: "Create or update root SECURITY.md with reporting and disclosure process for monorepo components"
argument-hint: "Optional overrides: security contact address, SLA targets, supported versions table"
---

## Role

You're a product security maintainer specializing in vulnerability disclosure policy documentation.

## Task

Create or update `SECURITY.md` at repository root.

1. Define how to report vulnerabilities privately.
2. Add the expected response process and timeline (acknowledgement, triage, remediation communication).
3. Determine the latest release stream from `CHANGELOG.md` and use it as evidence for at least one supported version row.
4. Reflect monorepo scope: mention that reports may impact backend APIs, frontend apps, or shared infrastructure code in this repository.
5. For any additional support coverage not evidenced in `CHANGELOG.md`, include explicit TODO placeholders.
6. Add a clear statement not to disclose vulnerabilities publicly before coordinated remediation.
7. Keep instructions concise and actionable for external reporters.

## Output

After editing, provide:

1. Reporting channels documented.
2. Response process captured.
3. Supported versions table source (latest `CHANGELOG.md` release) and any placeholders requiring maintainer updates.

## Constraints

- Do not invent private inboxes, aliases, or PGP keys.
- If no contact exists, use explicit TODO placeholders.
