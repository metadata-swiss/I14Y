---
paths: ["tests/e2e-tests/**"]
---

# E2E tests (Playwright .NET + NUnit)

Use the `/e2e-test` skill to write or fix a test here.

- **Projects** (under `tests/e2e-tests/src/`):
  - `Bfs.Iop.Admin.Testautomation` covers admin-ui.
  - `Bfs.Iop.Public.Testautomation` covers public-ui.
  - `Bfs.Iop.Test.Abstraction` holds the shared `Wrapper`, `EiamLogin` and `Listener`.
- **Not in CI, and not in `backend/i14y.slnx`.** Build with `dotnet build tests/e2e-tests/src/Bfs.Iop.Admin.Testautomation/Bfs.Iop.Admin.Testautomation.sln`.
- **Tests run against the deployed DEV or ABN environments through a real EIAM login.** Credentials and the TAN come from user secrets or environment variables. Claude must never type, store or log them.
