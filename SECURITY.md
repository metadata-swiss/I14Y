# Security Policy

## Supported Scope

This repository is a monorepo. Security reports may involve:

- Backend under `backend/src`
- Frontend under `frontend/public-ui` and `frontend/admin-ui`
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

The email above is the repository security contact channel currently evidenced in project governance files.

## Disclosure Process

1. Acknowledgement target: within 5 business days
2. Triage and impact assessment: as quickly as possible after acknowledgement
3. Remediation planning: coordinated with maintainers of affected components
4. Communication: coordinated disclosure after a fix or mitigation is available

## Supported Versions

`CHANGELOG.md` currently does not define formal release lines.

Use explicit TODO placeholders until release support policy is documented.

| Version line | Supported                                         |
| ------------ | ------------------------------------------------- |
| TODO         | TODO: define supported release lines and windows. |

## Coordinated Disclosure

Please keep vulnerability details private until the team confirms that public disclosure is safe.

## Third-Party Dependency Risk

Vulnerabilities in third-party dependencies (backend NuGet packages and frontend npm packages, including transitive dependencies) may impact shipped services and applications. Reports affecting dependency chains are in scope.
