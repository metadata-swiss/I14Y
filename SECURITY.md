# Security Policy

## Supported Scope

This repository is a monorepo. Security reports may involve:

- Backend under `src/i14y`
- Frontend under `src/ui/public-ui` and `src/ui/admin-ui`
- Shared build and deployment assets in repository root and `.github/workflows`

## Reporting a Vulnerability

Please report vulnerabilities privately by email:

i14y@bfs.admin.ch

Please include:

- Affected component and version/commit
- Reproduction steps or proof of concept
- Potential impact
- Suggested mitigation (if known)

Do not open public issues for undisclosed vulnerabilities.

## Disclosure Process

1. Acknowledgement target: within 5 business days
2. Triage and impact assessment: as quickly as possible after acknowledgement
3. Remediation planning: coordinated with maintainers of affected components
4. Communication: coordinated disclosure after a fix or mitigation is available

## Supported Versions

Formal support windows per release line are not yet documented in this repository.

| Version line | Supported                                         |
| ------------ | ------------------------------------------------- |
| 2.x          | Yes (current line configured in `GitVersion.yml`) |

## Coordinated Disclosure

Please keep vulnerability details private until the team confirms that public disclosure is safe.
