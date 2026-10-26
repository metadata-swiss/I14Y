# I14Y

I14Y is the Swiss federal interoperability platform. It is a metadata catalog for datasets, data services, public services, concepts and code lists, published as DCAT/RDF.

The stack is:
- a .NET 10 backend made of several HTTP APIs,
- two Angular frontends: `admin-ui` for data stewards and `public-ui` for the public,
- PostgreSQL, a Fuseki triple store and Elasticsearch for storage and search.

## Where to look first
- **Code graph (`graphify-out/`, if present):** a local AST graph of the code base. For "who uses X" or "what does X touch", query it before grepping or opening files:
  - `graphify explain "<Symbol>"` lists a symbol's references, implementations and methods, with `file:line`.
  - `graphify affected "<Symbol>" --depth 1` lists everything that depends on it.
  - When a name is ambiguous, use `path/to/File.cs::Symbol`.
  - Do not read `GRAPH_REPORT.md` in full; it is about 18k tokens. Free-text `graphify query` (and `/graphify query`) matches keywords and is unreliable. This takes precedence over the user-level graphify skill's "query first" advice.
  - **Limit:** MediatR `Send` is resolved at runtime, so the graph has no controller → handler edges. Follow the `*Command` → `*CommandHandler` naming instead.
  - **Rebuild after big changes** with `graphify extract . --code-only --no-cluster`. It is local, needs no API key, and takes about 30 s.
  - **Setup:** `pip install graphifyy` into a venv on Python 3.13 or older (3.15 has no tree-sitter wheels yet). If `graphify` is not on PATH, call `<venv>/Scripts/graphify.exe`.
- **Area guidance lives in `.claude/rules/`.** Each file loads automatically when a file matching its `paths` is touched:
  - `backend.md` (`backend/**`): request flows, persistence and the API compatibility checklist.
  - `csharp-conventions.md` (`**/*.cs`): the C# conventions, for the backend and the e2e tests.
  - `frontend.md` (`frontend/**`): Angular rules and commands.
  - `e2e.md` (`tests/e2e-tests/**`): the e2e suite.

## Module map (`backend/src`)
| Area | Role | Talks to |
|---|---|---|
| `Core` | System of record. MediatR commands, EF Core (Npgsql), Fuseki (`Core.LinkedData`), Blob storage (`Core.FileStorage`) | AuditTrail, IndexSearch over HTTP |
| `Data` | `DataAccess.Abstractions` (models, exceptions) and `DataAccess.Relational` (DbContext, entities, migrations, services, validators, mappings) | used by Core and IndexSearch |
| `Admin` | Backend-for-frontend for admin-ui. MediatR + Mapster + FluentValidation pipeline. Clients for Geocat, Lindas and OpenData | Core through the generated `IIopCoreApiClient` |
| `Partner` | External partner API | Core through `IIopCoreApiClient` |
| `Iri` | IRI resolver | Core through `IIopCoreApiClient` |
| `AuditTrail` | Change history kept in the triple store | called by Core |
| `IndexSearch` | Elasticsearch indexing and search. Internal, with no authorization | called by Core. Reads `DataAccess.Relational` directly |
| `Infrastructure` | `Common`, `Common.Api` (response attributes), `Security`, NSwag `ApiClient.Generator` | shared |

- **Messaging:** there is no message bus. Core queues changes on in-process `System.Threading.Channels` queues, and `BackgroundService` dispatchers push them to AuditTrail and IndexSearch with retries.
- **Source of truth:** `backend/i14y.slnx` lists the real projects. A folder under `backend/src` with no `.csproj` is a local build leftover, not part of the code base.

## Commands
```bash
dotnet build backend/i14y.slnx -c Release
dotnet test backend/i14y.slnx -c Release
dotnet run --project backend/src/Core/Bfs.Iop.Core.Api/Bfs.Iop.Core.Api.csproj
docker compose up --build        # full local stack; `docker compose down -v` to reset
```
- **CI** (`.github/workflows/quality-gates.yml`): these jobs run in parallel, each only when its paths changed:
  - builds the six APIs and runs every `*Tests.csproj`,
  - the vulnerable-package, Gitleaks, license and CodeQL checks,
  - `npm ci && npm run build-prod` for each UI.
- **E2E tests:** they are not in `i14y.slnx` and not in CI; see `.claude/rules/e2e.md`.

## Generated code: never edit by hand
- **Locations:** `backend/src/*/*.ApiClient/Generated`, `build/ts-client/generated` and `frontend/*/api-client/lib/generated`.
- **To regenerate:** run the matching `*.Api.ClientGenerator` project, then `npm run api:generated` in the UI.

## Working rules
- Keep changes as small as the task allows. Do not touch unrelated code: no renames, refactors, reformatting or error-handling changes outside the task.
- **PR size:** aim for at most about 400 changed lines of hand-written code per PR. The median recent PR is about 105 lines, and 69% are 400 or fewer.
  - Not counted: generated clients, migrations, lockfiles and i18n JSON.
  - When a change is bigger, split it into consecutive PRs, either by layer (Data + migration → Core → Admin + regenerated clients → UI) or with the refactor first and the feature after.
  - Never mix a refactor with a feature in the same PR.
- Preserve the existing architecture. Do not add interfaces, factories, generic repositories, helper classes or configuration options for hypothetical future needs.
- Do not add dependencies or swap technologies without a concrete reason.
- Carry intent in names, not comments. Add a comment only for a non-obvious *why*.
- Check API changes for backward compatibility: JSON names, nullability, status codes and generated clients.
- Verify your work with `dotnet build` / `dotnet test` (backend) or `ngc` (frontend) before reporting it done.

## Skills
- `/i14y-review` reviews the current branch against the team's review rules.
- `/e2e-test` writes or fixes a Playwright end-to-end test.
