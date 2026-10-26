---
name: e2e-test
description: Write, extend, or fix an I14Y end-to-end UI test (Playwright .NET + NUnit) in tests/e2e-tests for admin-ui or public-ui. Use when asked to add e2e coverage for a feature, automate a manual test case, or repair a flaky or failing e2e test.
---

# I14Y end-to-end tests

The tests live in `tests/e2e-tests/src`. They drive the **deployed** DEV or ABN environment through a real EIAM login.

## Before writing anything
1. Find the closest existing test for the same resource type and copy its shape:

   | Area | Existing test |
   |---|---|
   | Concepts | `Bfs.Iop.Admin.Testautomation/ConceptTests/CrudConceptTests.cs` |
   | Datasets | `.../DatasetTests/CrudDatasetTests.cs`, `CreateDeleteMaximalDataset.cs` |
   | Data services | `.../DataServiceTests/CrudDataServiceTests.cs` |
   | Public services | `.../PublicServices/PublicServicesTest.cs` |
   | Public UI | `Bfs.Iop.Public.Testautomation/...` |

   For a new admin-ui fixture, start from `Bfs.Iop.Admin.Testautomation/ConceptTests/ConceptIriTests.cs`. It is the smallest complete fixture (215 lines): setup, `Listener`, ordered `[Test, Order(n)]` steps, and a final delete through the UI (`StandardTask.DeleteConcept` plus the confirm dialog). Read the larger CRUD test only for the field-filling steps your feature needs.

2. Read the feature's Angular template to find the element `id`s the test will use. Do not select by CSS class or translated text.

### When an element has no id
Add it in the same change, in two places:
1. **The template:** add an `id` attribute in `frontend/admin-ui/src/app/**/<component>.html`. Follow the existing kebab-case style in `Constants/Concepts.cs`, e.g. `title-concept-de` or `create-concept`.
2. **The constants:** add a matching `public static readonly string` to `Constants/<Feature>.cs`.

List every added `id` in your summary. This is a frontend change, so it needs a frontend reviewer.

## Structure
- The fixture inherits the project's `Helpers/PlaywrightSetup`. Login, the browser, video and trace recording are already handled there, in `[OneTimeSetUp]`.
- Mark the fixture `[TestFixture]` and `[Parallelizable(ParallelScope.None)]`, because tests share one logged-in page.
- In `[SetUp]`, create a unique identifier from `TimeStamp`, then `new Listener(Page)` and `RecognizeApiErrors()`.
- In `[TearDown]`, `await _listener.WaitForResponseHandlingAsync()`, write `GetLastErrorMessage()` if there were API errors, then reset and dispose the listener.
- A CRUD flow is one method per step: `[TestCaseSource(nameof(TestCases)), Order(n)] public async Task Should<Action>Successfully(...)`, ending with a delete step.
- **Element ids:** element ids are constants in the project's `Constants/<Feature>.cs`. Never write raw strings in the test.
- **Form filling:** filling an edit form goes in the feature's `Helpers/*EditMaskHelper.cs`, so tests stay short. Navigation and common steps live in `Shared/StandardTask.cs`.
- **Interact through `Actions` (the `Wrapper`):** `ClickButtonById`, `FillInputById`, `SelectOptionById`, `SelectFirstAutocompleteById`, `ReadInput`, `ReadMatSelect`, `ValidateDivContent`, and so on. Add a new `Wrapper` method only when no existing one fits.
- **Cleanup:** a failed run must not leave orphaned resources on DEV.
  - Every CRUD flow ends with a delete step through the UI, as in `CrudConceptTests`.
  - For data services and public services, `Shared/DeleteTests.cs` can also delete through the Core API. It authenticates with the bearer token from `Actions.ReadLocalStorage(Navigation.AccessTokenKey)`; see `CrudDataServiceTests` and `PublicServicesTest`. It also has a dataset helper. It has no concept endpoint, so concepts rely on the UI delete step.

## Waiting
- **Do not add new `Wait500` / `Wait1000` / `Task.Delay` calls.** Fixed waits cause most of the flakiness in this suite.
- Use instead:
  - `Actions.WaitForSpinnerToDisappear()`
  - `Actions.WaitForElementToBeStable(...)`
  - `Actions.WaitForInputValidationById(...)`
  - `Actions.WaitUntilUrl(...)`
  - `Actions.WaitForNotificationToDisappear()`
  - Playwright auto-waiting on `ILocator`, or `Expect(locator).ToBeVisibleAsync()` / `ToHaveValueAsync(...)`. This needs `using static Microsoft.Playwright.Assertions;`, which is not used in the suite yet.
- Leave the existing fixed waits in place unless you are fixing that exact test.

## Assertions
- Use NUnit `Assert.That(actual, Is.EqualTo(expected), "<what was being checked>")`, or Playwright `Expect(...)` for UI state.
- Assert what the user would see after a reload (`Actions.Reload()`), not only the form state just after saving.

## Credentials and environments
- **Never write credentials into files.** `PASSWORD`, `TAN` and `UserName` come only from user secrets (`dotnet user-secrets set "PlaywrightConfig:PASSWORD" ...`) or environment variables. Never write them into `appsettings*.json`, code, logs or the chat. Claude does not type them; the user sets them.
- The environment comes from `DOTNET_ENVIRONMENT`, through `src/test.DEV.runsettings` / `src/test.ABN.runsettings`.

## Build and run
```bash
dotnet build tests/e2e-tests/src/Bfs.Iop.Admin.Testautomation/Bfs.Iop.Admin.Testautomation.sln
dotnet test tests/e2e-tests/src/Bfs.Iop.Admin.Testautomation/Bfs.Iop.Admin.Testautomation.csproj --settings tests/e2e-tests/src/test.DEV.runsettings --filter "FullyQualifiedName~CrudConceptTests"
```
- **Always build.**
- **Run only if the user confirms.** The run creates and deletes real data on DEV and needs their secrets configured.
- **Debugging:** set the environment variables `PlaywrightConfig__HeadLess=false`, `PlaywrightConfig__RecordVideo=true` and `PlaywrightConfig__TraceTests=true`. Traces go to `RESULTS_DIR` (default `TestResults`) as `trace.zip`.

## Running ordered tests
- **Steps depend on each other.** CRUD steps share state through fields and `Order(n)`: delete needs create to have run. Filtering on a single method (`~ShouldDeleteConceptSuccessfully`) fails or deletes nothing.
- **Filter on the whole fixture**, e.g. `--filter "FullyQualifiedName~CrudConceptTests"`.
- **Login happens once per fixture.** One fixture is the smallest unit worth running against DEV.

## Fixing a flaky or failing test
1. **Reproduce with tracing:** run the fixture once with `PlaywrightConfig__TraceTests=true`, then open the trace with `pwsh bin/Debug/net10.0/playwright.ps1 show-trace <RESULTS_DIR>/trace.zip`.
2. **Find the failing action.** Then classify the failure:
   - **Missing id:** the element's `id` was renamed or removed in the template. Fix the constant or the template.
   - **Timing:** the action ran before the UI was ready. Replace the fixed `WaitNNN` in front of it with the matching wait helper from "Waiting".
   - **Leftover data:** an earlier run left data behind, so search counts or names collide. Clean up and check that the delete step runs.
   - **Real bug:** the application is wrong. Report it; do not make the test tolerate it.
3. **Change only that test.** Don't touch other tests' waits in the same change.

## Done means
- The new test builds, ids come from constants, it adds no fixed waits, and it ends with a cleanup step.
- The run command filters on the whole fixture.
- Report what the test covers, plus any `id`s you added to Angular templates.
