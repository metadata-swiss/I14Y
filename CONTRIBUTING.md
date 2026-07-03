# Contributing

Thank you for contributing to I14Y.

## Scope

This repository is a monorepo with:

- Backend .NET projects in `src/i14y`
- Frontend Angular apps in `src/ui/public-ui` and `src/ui/admin-ui`

## How To Contribute

1. Open an issue for bugs, regressions, or feature requests.
2. Create a branch from `main` for your change.
3. Keep the change focused (backend, frontend app, API client generation, or shared infra).
4. Run relevant checks locally.
5. Maintainers open a pull request with a clear description and testing notes.

## External Contributions (Current Phase)

This repository is being prepared for open source publication, but external pull requests are not accepted yet.

At this stage:

- External contributors can open issues to report bugs or suggest improvements.
- Pull requests are currently limited to maintainers of this repository.
- This policy will be updated when external PRs are officially enabled.

## Pull Request Checklist

- Change is scoped and documented.
- Relevant build/test/lint commands pass locally.
- No secrets or environment-specific credentials are committed.
- Documentation is updated when behavior changes.

Recommended PR description format:

- What changed
- Why it changed
- How it was validated
- Risks or follow-up actions

Note: the checklist above applies to maintainer pull requests during the current phase.

## Documentation Policy

- Repository-level documentation is kept in the root files.
- If component-level docs are updated, reflect relevant changes in the root documentation as well.

## Local Validation

Backend:

```bash
dotnet restore src/i14y/i14y.slnx
dotnet build src/i14y/i14y.slnx -c Release
dotnet test src/i14y/i14y.slnx -c Release
```

Frontend public UI:

```bash
cd src/ui/public-ui
npm ci
npm run lint
npm run build
```

Frontend admin UI:

```bash
cd src/ui/admin-ui
npm ci
npm run lint
npm run build
```

API npm client generation project:

```bash
dotnet run --project src/i14y/Bfs.Iop.Admin.Api.ClientGenerator/Bfs.Iop.Admin.Api.ClientGenerator.csproj
cd src/i14y/bfs-iop-admin-ui
npm ci
npm run build
```

## Branching and Releases

Repository versioning and release branch behavior are configured in `GitVersion.yml` and release workflows in `.github/workflows`.

Current evidence in this repository:

- Main branch pattern: `main` or `master`
- Release-support branch pattern: `releases/*`

Commit-message and branch naming policy details are maintainer-defined. If your team uses additional conventions, document them in PR templates or internal contribution notes.

## Conduct and Security

- Please follow `CODE_OF_CONDUCT.md`.
- To report vulnerabilities, use `SECURITY.md` instead of public issues.

## License

By contributing, you agree that your contributions are licensed under the repository `LICENSE`.
