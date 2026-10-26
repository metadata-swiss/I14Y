---
paths: ["backend/**"]
---

# Backend (.NET 10)

C# code conventions are in `.claude/rules/csharp-conventions.md`, which loads for any `.cs` file.

## Core request flow (`POST /api/Concepts`)
| Layer | File |
|---|---|
| Controller: one-line `_mediator.Send(new XCommand(...), cancellationToken)` | `backend/src/Core/Bfs.Iop.Core.Api/Controllers/ConceptsController.cs` |
| Command: `public sealed record CreateIopConceptCommand(IopConceptInputModel Input) : IRequest<Guid>` | `backend/src/Core/Bfs.Iop.Core.Abstractions/Commands/IopConcepts/CreateIopConceptCommand.cs` |
| Handler: calls the service, then the audit-trail and search-index notifiers | `backend/src/Core/Bfs.Iop.Core/CommandHandlers/IopConcepts/CreateIopConceptCommandHandler.cs` |
| Service: EF Core, `PublishableEntityServiceBase<T>` | `backend/src/Data/Bfs.Iop.DataAccess.Relational/Services/IopConceptsService.cs` |
| Validation: the service calls its `I*ValidationService`, which wraps the FluentValidation `IValidator<T>`s | `backend/src/Data/Bfs.Iop.DataAccess.Relational/Validation/Services/IopConceptsValidationService.cs` |
| Mapping: hand-written extension methods (no Mapster in Core/Data) | `backend/src/Data/Bfs.Iop.DataAccess.Relational/Mappings/*MappingExtensions.cs` |
| Exception → ProblemDetails (Hellang `options.Map<...>`) | `backend/src/Core/Bfs.Iop.Core.Api/Startup.cs`, `backend/src/Core/Bfs.Iop.Core.Api/Exceptions/DatabaseErrorMapper.cs` |

**Every write handler must notify** `IAuditTrailNotifierService` and `ISearchIndexNotifierService` after a successful write. Forgetting this leaves the audit trail and the search index stale.

## Admin (BFF) request flow
| Layer | File |
|---|---|
| Handler: maps with Mapster, calls Core through `IIopCoreApiClient` | `backend/src/Admin/Bfs.Iop.Admin.Business/Commands/ConceptInput/CreateCommandHandler.cs` |
| Validation: FluentValidation `IPipelineBehavior` | `backend/src/Admin/Bfs.Iop.Admin.Business/Validation/ValidationBehavior.cs` |
| Mapping: Mapster `*MappingRegister` | `backend/src/Admin/Bfs.Iop.Admin.Business/Mappings/` |
| DI registration | `backend/src/Admin/Bfs.Iop.Admin.Business/Extensions/ServiceCollectionExtensions.cs` |

When a Core contract changes, regenerate `Core.ApiClient` (run `Bfs.Iop.Core.Api.ClientGenerator`) and then update the Admin and Partner callers. If Admin's contract changes, also regenerate the TypeScript client.

## Persistence
- **EF Core migrations** live in `backend/src/Data/Bfs.Iop.DataAccess.Relational/Migrations`. Add one with `dotnet ef migrations add <Name>`, using `Bfs.Iop.DataAccess.Relational` as the project and `Bfs.Iop.Core.Api` as the startup project. Core applies migrations and seeds sample data on startup.
- **Triple store (Fuseki):** use `Bfs.Iop.Core.LinkedData`. The local Fuseki is empty, so features that depend on RDF structure cannot be verified locally.
- **Unit tests use EF InMemory** (`backend/src/Data/Bfs.Iop.DataAccess.Relational.UnitTests/Helpers/TestHelper.cs`), which cannot prove that an `Include` is present. Guards on navigation properties need real Npgsql tests.

## API compatibility checklist (before changing a controller or contract)
- Request and response models, JSON property names, and optional/nullable/required properties.
- HTTP status codes, validation behaviour and the shape of error responses.
- The generated C# and TS clients and their consumers. Serialized values must match the types the consumers expect.
- Prefer an additive, backward-compatible change over a redesign.

## Review focus for C#
- Correct async/await, with `cancellationToken` passed all the way down.
- Correct DI lifetimes: no scoped service captured by a singleton or a `BackgroundService`.
- Nullability: no `!` that hides a real null. No unsafe casts.
- Exceptions mapped to the right ProblemDetails status.
- Race conditions in the dispatchers and the index writers.
