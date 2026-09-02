# Copilot Instructions

## 1. General Review Rules

When reviewing or modifying code:

* Prioritize correctness over code style.
* Do not suggest changes based only on personal preference.
* Do not refactor working code unless there is a clear bug, performance issue, type-safety problem, or maintainability problem.
* Preserve the repository’s existing architecture and established patterns.
* Keep changes as small and focused as possible.
* Do not modify unrelated code.
* Do not introduce new dependencies unless they are clearly necessary.
* Do not replace an existing implementation with a different technology without a strong, specific reason.
* Prefer the simplest solution that correctly solves the requested problem.
* Pay particular attention to type mismatches, especially in `.ts` files.

## 2. Existing Code Must Be Preserved

Be especially careful with existing functions and business logic.

If a function is not directly related to the requested change:

* Do not rewrite it.
* Do not rename it.
* Do not change its behavior.
* Do not simplify or refactor it.
* Do not change its error handling.

AI-generated code is not a reason to rewrite surrounding existing code.

When reviewing a Pull Request, explicitly identify unnecessary changes to existing code.

## 3. Backend - .NET / C#

Follow the existing .NET architecture and coding patterns.

Review carefully for:

* Incorrect async/await usage.
* Missing cancellation handling where appropriate.
* Incorrect exception handling.
* Nullability problems.
* Incorrect dependency injection lifetimes.
* Unnecessary service or repository changes.
* Breaking API changes.
* Incorrect HTTP status codes.
* Incorrect validation.
* Race conditions.
* Incorrect state management.
* Type mismatches between request models, response models, domain models, DTOs, and persistence models.
* Unsafe casts or conversions that can cause runtime failures.
* Inconsistent nullable and non-nullable types.

Do not introduce a new architectural pattern when an existing repository pattern already solves the problem.

Do not add abstractions, layers, or patterns unless the change has a concrete need for them.

## 4. API Compatibility

Before recommending an API change, check whether it is backward compatible.

Pay particular attention to:

* Request models.
* Response models.
* JSON property names.
* HTTP status codes.
* Validation behavior.
* Error responses.
* Generated API clients.
* Type mismatches between API contracts and TypeScript or C# models.
* Optional, nullable, and required properties.
* Differences between serialized values and the types expected by consumers.

Do not modify generated client code manually unless explicitly requested.

Do not recommend redesigning an API when a smaller compatible change solves the problem.

## 5. Angular / TypeScript

For Angular and TypeScript code:

* Follow the existing Angular architecture.
* Prefer existing services and components over introducing duplicates.
* Avoid unnecessary RxJS changes.
* Avoid unnecessary subscriptions.
* Check subscription cleanup and observable lifetimes.
* Check reactive forms carefully.
* Do not change generated API clients manually.
* Avoid unrelated formatting or refactoring.
* Verify that template changes are compatible with the component’s form and state model.
* Do not introduce new state-management patterns unless the existing approach cannot support the requested change.
* Pay close attention to type mismatches in all `.ts` files.
* Verify that function arguments match the declared parameter types.
* Verify that return values match the declared return types.
* Check assignments between API responses, DTOs, interfaces, form values, component state, and service models.
* Check for mismatches between `string`, `number`, `boolean`, `Date`, enums, arrays, objects, and nullable or optional values.
* Check for mismatches between `Observable<T>`, `Promise<T>`, and plain `T`.
* Check generic types such as `Observable<T>`, `Subject<T>`, `FormControl<T>`, `FormGroup<T>`, and collection types.
* Check whether TypeScript type assertions, non-null assertions, or `any` are hiding an actual mismatch.
* Do not silence a type error with `any`, `as`, `!`, or compiler configuration changes unless the type is demonstrably correct and the smallest appropriate fix is documented.
* Verify that Angular templates use values compatible with the component property types and method signatures.
* Check that form controls and form values use the same types as the models they populate.
* Check that optional properties are handled before use.
* Check that API-generated types match the actual backend payload, including property names, nullability, and collection shapes.
* Treat compile-time type errors and likely runtime type mismatches as concrete review findings.

## 6. Tests

Do not check, request, or evaluate test coverage unless the user explicitly asks for it.

Do not report missing tests as a review finding.

Do not require new tests for behavior changes, bug fixes, API changes, edge cases, or error handling.

## 7. Pull Request Review Priorities

When reviewing a Pull Request, prioritize findings in this order:

1. Data corruption or data loss.
2. Incorrect business logic.
3. Type mismatches that cause compilation failures or runtime errors.
4. Breaking API behavior.
5. Security vulnerabilities.
6. Performance problems.
7. Concurrency problems.
8. Maintainability problems.
9. Style issues.

Do not report minor style issues when more important correctness problems exist.

## 8. Review Finding Requirements

Only report an issue when there is a concrete technical reason.

Each finding should explain:

* What is wrong.
* Why it is a problem.
* The likely impact.
* The smallest appropriate fix.

For `.ts` type-related findings, include:

* The incompatible types.
* The location where the mismatch occurs.
* Whether the issue causes a compile-time error or a runtime risk.
* The smallest type-safe correction.

Avoid vague comments such as:

* “This could be improved.”
* “Consider refactoring this.”
* “This is not clean.”
* “Use a better architecture.”

Instead, identify the specific technical problem and recommend the smallest fix that addresses it.

Do not create findings solely because code could be made more abstract, elegant, modern, or consistent with a different design preference.

## 9. Avoid AI Over-Engineering

Avoid AI over-engineering. Do not add complexity unless the requested change clearly requires it.

Do not introduce unnecessary:

* Interfaces without multiple implementations or a clear immediate need.
* Generic repositories.
* Service layers.
* Factories.
* Helper classes.
* Design patterns.
* Configuration options.
* Validation layers.
* Error-handling frameworks.
* State-management mechanisms.
* Extension points.
* Abstractions for one-time operations.

Prefer a direct, local change when it is sufficient.

Do not generalize code for hypothetical future requirements.

Do not split a small change across multiple files unless there is a concrete benefit.

Do not replace simple code with a more complex pattern merely because the pattern is considered more scalable or architecturally pure.

Before adding an abstraction, ask:

> Is this required for the current change, or is it only preparing for a possible future need?

If it is only for a possible future need, do not add it.

## 10. Scope Control

A Pull Request should contain only the changes required for its stated purpose.

Flag:

* Unrelated refactoring.
* Large formatting-only changes.
* Renaming unrelated variables or classes.
* Changes to unrelated functions.
* Changes to unrelated error handling.
* Dependency upgrades unrelated to the feature.
* Architecture changes unrelated to the feature.
* New abstractions that are not required by the feature.
* Generalization for hypothetical future use.
* Changes that weaken or bypass TypeScript type safety without a concrete justification.
* Changes that introduce `any`, unsafe casts, non-null assertions, or compiler suppressions to hide type mismatches.

For AI-generated Pull Requests, pay particular attention to scope creep, unnecessary complexity, and type-safety regressions.

Do not recommend expanding the scope of a Pull Request unless the current implementation has a concrete correctness, security, performance, type-safety, or maintainability problem.

## 11. Final Review Question

Before considering a Pull Request acceptable, ask:

> Does this change solve the requested problem with the smallest reasonable implementation while preserving existing behavior everywhere else?

Also ask:

> Are all `.ts` values, parameters, return types, API models, form values, observables, and component state type-compatible without unsafe casts or hidden mismatches?

If the answer to either question is no, identify the specific regression risk, type mismatch, unnecessary complexity, or scope problem.
