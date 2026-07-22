# Contributing

Thank you for contributing to I14Y.

## Repository Layout

- Backend (.NET): `backend/src/...`
- Frontend Angular apps:
	- `frontend/public-ui`
	- `frontend/admin-ui`
- Generated admin TypeScript API client source: `build/ts-client/generated`

Use these paths to place changes in the correct component.

## Preferred Contribution Workflow

1. Report or discuss first:
	 - Open an issue for bugs, regressions, or feature requests.
	 - For unclear scope, start with an issue before opening a pull request.
2. Implement the change on a dedicated branch.
3. Keep the change focused and component-scoped.
4. Run relevant local validation commands.
5. Open a pull request with context and validation notes.
6. Address review feedback with incremental commits and update the PR description if scope changes.

## Pull Request Checklist

- Change is scoped to the intended component(s).
- Local build/lint/test checks pass for affected areas.
- No secrets or environment-specific credentials are committed.
- Documentation is updated when behavior or setup changes.

Recommended PR description:

- What changed
- Why it changed
- How it was validated
- Risks, rollout notes, or follow-ups

## Technical Expectations

- Backend changes belong under `backend/src/...` and should be validated via solution-level .NET commands.
- Frontend changes belong under `frontend/public-ui` or `frontend/admin-ui` and should include app-level npm validation.
- Admin API client generation is distinct from runtime frontends:
	- Generator project: `backend/src/Admin/Bfs.Iop.Admin.Api.ClientGenerator`
	- Generated output: `build/ts-client/generated`
	- Frontend sync command: `npm run api:generated` inside each frontend app

## Local Validation

Backend:

```bash
dotnet restore backend/i14y.slnx
dotnet build backend/i14y.slnx -c Release
dotnet test backend/i14y.slnx -c Release
```

Public UI:

```bash
cd frontend/public-ui
npm ci
npm run api:generated
npm run lint
npm run build
```

Admin UI:

```bash
cd frontend/admin-ui
npm ci
npm run api:generated
npm run lint
npm run build
```

Regenerate admin API TypeScript client:

```bash
dotnet run --project backend/src/Admin/Bfs.Iop.Admin.Api.ClientGenerator/Bfs.Iop.Admin.Api.ClientGenerator.csproj
```

## Branch and Commit Naming

Current branch patterns:

- Main branch: `main`
- Release branch pattern: `release/*` and `releases/*` (workflows)
- Common working branch prefixes: `feature/`, `fix/`, `tasks/` (with occasional one-off branch names)

Recommended working branch format:

- `<type>/<ticket>_<short-description>`
- `type`: `feature`, `fix`, `tasks`, or `hotfix`
- examples: `feature/781_create_export_buttons`, `fix/accept_empty_contactPoints.kind`, `tasks/444/expand_partner_post_array_endpoints`

Common commit message patterns:

- Common prefixes: `feat:`, `fix:`, `chore:`
- Frequent ticket/issue references in subject, including `I14Y-ch/planning#...`

Recommended commit subject format:

- `<type>: <organization>/<repo>#<ticket> <short imperative summary>`
- Use `I14Y-ch/planning#<ticket>` for planning references.
- examples: `feat: I14Y-ch/planning#730 select and order Azure component`, `fix: I14Y-ch/planning#799 handle empty ContactPoint.kind`

## Governance Links

- Code of Conduct: [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)
- Security Policy: [SECURITY.md](SECURITY.md)
- Changelog: [CHANGELOG.md](CHANGELOG.md)
- License: [LICENSE](LICENSE)

For security vulnerabilities, follow [SECURITY.md](SECURITY.md) and avoid public disclosure in issues.
