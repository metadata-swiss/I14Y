---
paths: ["**/*.cs", "**/*.csproj"]
---

# C# conventions

These conventions describe what the code base already does. They were measured over roughly 1,350 non-generated files. Where the code base is split, the rule is **follow the surrounding file** until the team decides; those cases are listed at the end.

## Project setup
- Target `net10.0` and set `<Nullable>enable</Nullable>` in every new project.
- Use file-scoped namespaces (`namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;`). The namespace matches the folder path.
- Test projects are siblings of the project they test, named `<Project>.UnitTests`, with folders that mirror the source.

## MediatR commands and handlers
- Write every request as a `public sealed record` with positional parameters, named `*Command`. Reads are commands too (`GetIopConceptCommand`); there are no `*Query` types.
  ```csharp
  public sealed record CreateIopConceptCommand(IopConceptInputModel Input) : IRequest<Guid>;
  ```
- Handlers are `internal sealed class <Command>Handler : IRequestHandler<TCommand, TResult>`.
- **Core:** commands live in `Core.Abstractions/Commands/<Feature>/`, and handlers in `Core/CommandHandlers/<Feature>/`.
- **Admin:** handlers live in `Admin.Business/Commands/<Feature>/{Create|Put|GetById}CommandHandler.cs`.
- **Keep handlers thin:** delegate to a service or API client, then notify the audit trail and the search index after a write.

## Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class ConceptsController : ControllerBase
```
- Return types are declared with the `Bfs.Iop.Common.Api` attributes (`[Ok(typeof(T))]`, `[NotFound]`, `[BadRequest]`), and controllers keep their `/// <summary>` docs because those feed Swagger.
- An action does one `_mediator.Send(...)` and has no business logic.
- Errors are exceptions mapped to ProblemDetails in `Startup`. Do not build error responses by hand in controllers.

## Dependencies and fields
- Use constructor injection into `private readonly` fields named `_camelCase`. Private `const` and `static readonly` fields are PascalCase (163 vs 37 with `_`).
- Register services through one `IServiceCollection` extension method per project (`AddIopCoreServices`, and so on). Do not register them in `Program.cs`.
- Pick the narrowest correct lifetime. Never capture a scoped service in a singleton or a `BackgroundService`. Create a scope instead.

## Async and cancellation
- Every I/O method is async and takes `CancellationToken cancellationToken` as its **last** parameter.
- Controller actions take it without a default (356 of 421 actions do). Below the controller, it is passed down to EF, `HttpClient` and the generated clients, never dropped.
- In new code, never use `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` or `async void` (except in event handlers). About 19 blocking calls already exist, e.g. in `GetAgentsStatisticsCommandHandler`. Don't copy them.

## Style
- Use `var` when the type is evident, or whenever you like; this is the code-base norm.
- Prefer early returns over nested `if`s, and always use braces.
- Use expression-bodied members only for one-liners.
- Log through `ILogger<T>` with message templates and PascalCase placeholders, never string interpolation:
  ```csharp
  _logger.LogWarning("Reindex of {ResourceType} {Id} failed after {RetryCount} retries", type, id, retries);
  ```
- Names carry intent; comments explain only a non-obvious *why*. Public API controllers keep XML docs, and other classes need none.
- Do not use `!` to silence nullability. Fix the flow or guard explicitly.
- Do not add an abstraction (interface, base class, factory, helper) without a second real use. Interfaces that exist for DI and mocking already follow the `IXService` pattern; follow it.

## Mapping and validation
- **Core and Data:** use hand-written `*MappingExtensions` static methods. Validation goes through a per-entity `I*ValidationService`, which wraps FluentValidation `IValidator<T>`s and is called explicitly by the service.
- **Admin:** use Mapster `*MappingRegister` classes and the FluentValidation `ValidationBehavior` pipeline.
- Do not mix the two styles inside one area.

## Unit tests
- **Libraries:** NUnit, NSubstitute and AwesomeAssertions. Do not add Moq, FluentAssertions or xUnit.
  ```csharp
  [TestFixture(TestOf = typeof(IopConceptsService))]
  internal sealed class IopConceptsServiceTests
  ```
- Database tests use EF InMemory through the project's `TestHelper` and call `EnsureDeleted` in `[TearDown]`. InMemory cannot prove an `Include`, so navigation guards need real Npgsql tests.
- When code depends on time, inject `TimeProvider` and use `FakeTimeProvider` in tests. Never use real delays.

## Undecided: follow the surrounding file
| Topic | Split in the code base |
|---|---|
| `Async` suffix | ~880 async methods without it (services, controllers), ~314 with it (notifiers, generated clients) |
| Constructor null guards | `?? throw new ArgumentNullException(nameof(x))` (310) vs `ArgumentNullException.ThrowIfNull` (327) vs none |
| Test names | `Given_X_When_Y_Then_Z` (Data/Core/Admin) vs sentence style (IndexSearch) vs `Map_X_To_Y_Ok` (Admin mappings) |
| `// Arrange / Act / Assert` comments | present in about half of the test files |
| `ImplicitUsings` | off in Admin.Api, Admin.Business, Core.Api and Core.ApiClient (explicit `using System;` there) |
| Handler `sealed` | 155 sealed, 45 not (mostly Admin). Seal new ones |
