---
paths: ["frontend/**"]
---

# Frontend (Angular): `admin-ui` and `public-ui`

## Commands (run inside `frontend/admin-ui` or `frontend/public-ui`)
```bash
npm ci
npm run api:generated   # copies build/ts-client/generated into api-client/lib/generated
npm run start
npm run build-prod
```
- **Linting is broken.** `ng lint` cannot run, and prettier flags every file. Verify type safety with `npx ngc -p tsconfig.app.json` instead. Do not mass-format files.
- **Builds can run out of memory.** An esbuild out-of-memory crash looks like a broken build. Free some RAM and retry before you start debugging.
- **Never edit `api-client/lib/generated` by hand.** Regenerate it from the backend instead: run `Bfs.Iop.Admin.Api.ClientGenerator`, then `npm run api:generated`.

## Rules
- Follow the existing component, service and form structure. Reuse existing services and components rather than creating duplicates.
- Do not introduce new state-management or RxJS patterns. Check subscription cleanup and observable lifetimes.
- **Type safety is a correctness issue.** The following must agree with each other:
  - API responses, DTOs, form values, `FormControl<T>` / `FormGroup<T>`, and component state,
  - function arguments and return types,
  - `Observable<T>`, `Promise<T>` and `T`,
  - `string`/`number`/`Date`/enum values, and nullable/optional properties.
- **Do not silence a type error** with `any`, `as`, `!` or tsconfig changes, unless the type is demonstrably correct. In that case, explain why.
- Templates must bind values that match the component's property types and method signatures.
- Translation keys must exist in every language file.
