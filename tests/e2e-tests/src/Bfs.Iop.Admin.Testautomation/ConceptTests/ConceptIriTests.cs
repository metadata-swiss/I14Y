using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Shared;
using Microsoft.Playwright;

namespace Bfs.Iop.Admin.Testautomation.ConceptTests;

/// <summary>
/// Tests for the concept IRI:
/// - Creates a public concept in IOP Admin and checks the generated IRI
/// - Opens the IRI and checks that the IRI service redirects to IOP Public
/// - Checks the identifier and the IRI shown in IOP Public
/// - Deletes the concept
/// IRI pattern: {BaseIriUrl}/concept/{concept-identifier}/version/{concept-version}
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class ConceptIriTests : PlaywrightSetup
{
    private string _nameConcept;
    private string _identifierConcept;
    private string _descriptionConcept;

    private string _conceptIri = string.Empty;
    private string _publicUrl = string.Empty;

    private StandardTask? _standardAction;
    private Listener? _listener;

    [SetUp]
    public void SetupInternal()
    {
        _nameConcept = ConceptData.NameConcept + "Iri" + TimeStamp;
        _identifierConcept = ConceptData.IdentificatorConcept + "Iri" + TimeStamp;
        _descriptionConcept = ConceptData.DescriptionConcept + "Iri";

        _standardAction = new StandardTask();

        _listener = new Listener(Page);

        _listener.RecognizeApiErrors();
    }

    [TearDown]
    public async Task CleanupAfterTestAsync()
    {
        if (_listener != null)
        {
            await _listener.WaitForResponseHandlingAsync();
            if (_listener.HasApiErrors())
            {
                TestContext.WriteLine(_listener.GetLastErrorMessage());
            }
            _listener?.ResetErrors();
        }
        _listener?.Dispose();
        _standardAction = null;
        _listener = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Creates a new concept in IOP Admin and sets its level to "Public", so that it is visible in IOP Public.
    /// </summary>
    [Test, Order(1)]
    public async Task ShouldCreatePublicConceptSuccessfully()
    {
        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenCreateConceptMask(Actions);

        await ConceptEditMaskHelper.FillConceptMinimal(Actions, _nameConcept, _descriptionConcept, _identifierConcept, CurrentDate);
        await ConceptEditMaskHelper.SetStringType(Actions);

        await _standardAction!.SaveAndCloseConcept(Actions);

        await Actions.WaitForSpinnerToDisappear();

        CheckApiError();

        await GoToConceptDetail();

        await _standardAction!.SetPublicLevelToPublic(Actions);

        CheckApiError();

        var level = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipLevelId);
        Assert.That(level, Is.EqualTo(StatusLevelVersion.PublicNameDe), $"level is not set correctly. {StatusLevelVersion.PublicNameDe} instead of {level}.");
    }

    /// <summary>
    /// Checks in IOP Admin that the IRI of the concept follows the pattern.
    /// </summary>
    [Test, Order(2)]
    public async Task ShouldShowConceptIriInAdminSuccessfully()
    {
        await _standardAction!.ChangeLanguageToGerman(Actions);

        await GoToConceptDetail();

        var version = await _standardAction!.ReadDiv(Actions, StatusLevelVersion.DivVersionId);
        var expectedIri = BuildConceptIri(_identifierConcept, version!.Trim());

        _conceptIri = await ReadIri();

        TestContext.Out.WriteLine($"IRI in IOP Admin: {_conceptIri}");
        Assert.That(_conceptIri, Is.EqualTo(expectedIri), "The IRI in IOP Admin does not follow the pattern.");
    }

    /// <summary>
    /// Opens the IRI of the concept and checks that the IRI service redirects to IOP Public.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldRedirectConceptIriToPublicSuccessfully()
    {
        Assert.That(_conceptIri, Is.Not.Empty, "The IRI was not read in IOP Public.");

        // BASE_PUBLIC_URL can itself redirect to another address, so we compare with the final address.
        await OpenUrl(BasePublicUrl);
        _publicUrl = Page.Url;
        TestContext.Out.WriteLine($"IOP Public address: {_publicUrl}");

        TestContext.Out.WriteLine($"Open IRI {_conceptIri}");
        await OpenUrl(_conceptIri);

        TestContext.Out.WriteLine($"Redirected to {Page.Url}");
        Assert.That(Page.Url, Does.Contain("/catalog/concepts/"), "The IRI was not redirected to a concept page in IOP Public.");
    }

    /// <summary>
    /// Checks in IOP Public that the identifier and the IRI of the concept have the right value.
    /// </summary>
    [Test, Order(4)]
    public async Task ShouldShowConceptIdentifierAndIriInPublicSuccessfully()
    {
        Assert.That(_publicUrl, Is.Not.Empty, "The IOP Public address was not read.");

        await _standardAction!.CheckDivPropertyById(Actions, "Identifier", Concepts.DetailIdentifierId, _identifierConcept);

        var publicIri = await ReadIri();

        TestContext.Out.WriteLine($"IRI in IOP Public: {publicIri}");
        Assert.That(publicIri, Is.EqualTo(_conceptIri), "The IRI in IOP Public is not the same as in IOP Admin.");
    }

    /// <summary>
    /// Goes back to IOP Admin, resets the level of the concept to "Unit" and deletes the concept.
    /// </summary>
    [Test, Order(5)]
    public async Task ShouldDeleteConceptSuccessfully()
    {
        await OpenUrl(BaseAdminUrl);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await GoToConceptDetail();

        await _standardAction!.ResetPublicLevelToInternal(Actions);

        CheckApiError();

        TestContext.Out.WriteLine($"Delete Concept {_identifierConcept}");

        await _standardAction!.DeleteConcept(Actions);

        await Actions.ClickButtonById(Test.Abstraction.Constants.Dialogs.ConfirmId);

        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear();

        CheckApiError();
    }

    private async Task OpenUrl(string url)
    {
        await Page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = WrapperConstants.DEFAULT_TIMEOUT
        });

        await Actions.WaitForSpinnerToDisappear();
    }

    private string BuildConceptIri(string identifier, string version)
    {
        return $"{BaseIriUrl}/concept/{identifier}/version/{version}";
    }

    /// <summary>
    /// Reads only the IRI text. The IRI block can also contain a copy button and a LINDAS link.
    /// </summary>
    private async Task<string> ReadIri()
    {
        var iri = await Page.Locator($"#{Concepts.DetailIriPermalinkId} span").First.TextContentAsync();

        return iri?.Trim() ?? string.Empty;
    }

    private async Task GoToConceptDetail()
    {
        // A new or changed concept is not always found in the search immediately.
        // The search waits 1 second before counting the results; if nothing is found, we wait 3 seconds and retry (max 3 retries).
        const int maxRetries = 3;

        await _standardAction!.GotoCatalog(Actions);
        var count = await _standardAction!.SearchCountConceptByName(Actions, _identifierConcept);

        for (var retry = 1; retry <= maxRetries && count == 0; retry++)
        {
            TestContext.Out.WriteLine($"Concept {_identifierConcept} not found yet, retry {retry}/{maxRetries} in 3 seconds.");
            await Actions.Wait3000();

            await _standardAction!.GotoCatalog(Actions);
            count = await _standardAction!.SearchCountConceptByName(Actions, _identifierConcept);
        }

        Assert.That(count, Is.GreaterThan(0), $"The concept {_identifierConcept} was not found in the catalog.");

        await Actions.ClickButtonById(Concepts.CatalogTableViewButton + "0");
        await Actions.WaitForSpinnerToDisappear();
    }

    private void CheckApiError()
    {
        var hasError = _listener!.HasApiErrors();
        Assert.That(hasError, Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage()}");
        if (hasError)
        {
            _listener.ResetErrors();
        }
    }
}
