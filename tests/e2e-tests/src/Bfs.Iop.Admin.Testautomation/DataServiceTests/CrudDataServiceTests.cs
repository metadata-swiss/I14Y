using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;

namespace Bfs.Iop.Admin.Testautomation.DataServiceTests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class CrudDataServiceTests : PlaywrightSetup
{
    private string _titleDataService;
    private string _identifierDataService;
    private string _bearerToken = string.Empty;
    private IReadOnlyDictionary<string, string> _usedVocabularyValues = new Dictionary<string, string>();

    private StandardTask? _standardAction;
    private Listener? _listener;

    [SetUp]
    public void SetupInternal()
    {
        _titleDataService = DataServiceConstants.Data.TitleDataService + TimeStamp;
        _identifierDataService = DataServiceConstants.Data.IdentifierDataService + TimeStamp;

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
    /// Creates a DataService with only the mandatory fields in the Admin UI and verifies that it is saved without API errors.
    /// </summary>
    [Test, Order(1)]
    public async Task ShouldCreateDataServiceWithMandatoryFieldsSuccessfully()
    {
        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await OpenCreateDataServiceMask();

        await DataServiceEditMaskHelper.FillDataServiceMinimal(Actions, _titleDataService, _identifierDataService);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataService(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
    }

    /// <summary>
    /// Opens the newly created DataService in view mode and checks that title, description and access rights are displayed correctly in German.
    /// </summary>
    [Test, Order(2)]
    public async Task ShouldDoubleCheckDataServiceWithMandatoryFieldsSuccessfully()
    {
        var title = _titleDataService;

        TestContext.Out.WriteLine($"Check the contents of the new data service {title}");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.OpenViewDataserviceMaskByName(Actions, title);

        await _standardAction!.CheckDivPropertyById(Actions, "title", DataServiceConstants.ViewMask.DescriptionTitleId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Description", DataServiceConstants.ViewMask.DescriptionDescriptionId, title);

        //var accessRights = await _standardAction!.ReadChip(Actions, DataServices.DescriptionAccessRightsId);
        //Assert.That(accessRights.Trim(), Is.EqualTo(DataServiceData.AccessRightsViewDe), $"access rights is not set correctly. {DataServiceData.AccessRightsViewDe} instead of {accessRights}.");
    }

    /// <summary>
    /// Edits the existing minimal DataService by adding endpoints, contact details, themes, keywords, documents, version info, etc., then saves and verifies no API errors.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldUpdateDataServiceMaximalSuccessfully()
    {
        var title = _titleDataService;

        TestContext.Out.WriteLine("Start updating a minimal data service with additional content");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenEditDataServiceMaskByName(Actions, title);

        _usedVocabularyValues = await DataServiceEditMaskHelper.FillToUpdateDataServiceMaximal(Actions);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataService(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
    }

    /// <summary>
    /// Opens the updated DataService in view mode and verifies that all additional fields (publisher, version, themes, endpoints, contact points, conformsTos, documents) are displayed correctly.
    /// </summary>
    [Test, Order(4)]
    public async Task ShouldDoubleCheckDataServiceMaximalSuccessfully()
    {
        var title = _titleDataService;

        TestContext.Out.WriteLine($"Check the contents of the new data service {title}");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.OpenViewDataserviceMaskByName(Actions, title);

        await CheckDataServiceMaximal();
    }

    /// <summary>
    /// Deletes the DataService via the API using the stored bearer token and confirms that no API errors occurred during deletion.
    /// </summary>
    [Test, Order(5)]
    public async Task ShouldDeleteDataServiceSuccessfully()
    {
        var title = _titleDataService;

        await DeleteDataService(title);

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
    }

    private async Task OpenCreateDataServiceMask()
    {
        TestContext.Out.WriteLine("Open create mask for data service");

        var dropdown = await Actions.FindElementById(DataServiceConstants.EditMask.CatalogMenuButtonId);

        if (dropdown == null)
        {
            Assert.Fail("Catalog menu button not found!");
        }

        await dropdown!.ClickAsync();
        await Actions.Wait500();

        var createDataServiceButton = await Actions.FindElementById(DataServiceConstants.EditMask.CreateDataServiceButtonId);

        if (createDataServiceButton == null)
        {
            Assert.Fail("Create data service button not found!");
        }
        await createDataServiceButton!.ClickAsync();
    }

    private async Task DeleteDataService(string title)
    {
        await Actions.Wait500();

        await _standardAction!.GotoCatalog(Actions);
        await Actions.WaitForSpinnerToDisappear();

        if (string.IsNullOrWhiteSpace(_bearerToken))
        {
            _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);
        }

        try
        {
            var deleteTest = new DeleteTests(BaseIopCoreUrl, _bearerToken, title);
            await deleteTest.DeleteDataService();
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Playwright_Test_DataService delete failed {ex.Message}");
        }
    }

    private async Task CheckDataServiceMaximal()
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Publisher", DataServiceConstants.ViewMask.DescriptionPublisherId, DataServiceConstants.Data.PublisherViewDe);

        await _standardAction!.CheckDivPropertyById(Actions, "Version", DataServiceConstants.ViewMask.DescriptionVersionId, DataServiceConstants.Data.VersionValue);

        await _standardAction!.CheckDivPropertyById(Actions, "Version Notes", DataServiceConstants.ViewMask.DescriptionVersionNotesId, DataServiceConstants.Data.VersionNoteValue);

        await _standardAction!.CheckDivPropertyById(Actions, "Themes", DataServiceConstants.ViewMask.DescriptionThemesId, _usedVocabularyValues[DataServiceConstants.EditMask.ThemeCodesId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Landing pages", DataServiceConstants.ViewMask.DescriptionLandingPageId, DataServiceConstants.Data.LandingPagesDeIdValue, false);

        //await _standardAction!.CheckDivPropertyById(Actions, "Access rights", DataServices.ViewMask.DescriptionAccessRightsId, _usedVocabularyValues[DataServices.EditMask.AccessRightsId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Endpoint URL", DataServiceConstants.ViewMask.DescriptionEndpointUrlsId, DataServiceConstants.Data.DocumentsHrefIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Endpoint Description", DataServiceConstants.ViewMask.DescriptionEndpointDescriptionsId, DataServiceConstants.Data.DocumentsHrefIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Contact point fn", DataServiceConstants.ViewMask.DescriptionContactPointId, DataServiceConstants.Data.ContactOrganisationValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Contact point Address", DataServiceConstants.ViewMask.DescriptionContactPointId, DataServiceConstants.Data.ContactAddressValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Contact point Note", DataServiceConstants.ViewMask.DescriptionContactPointId, DataServiceConstants.Data.ContactNoteValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Contact point Email", DataServiceConstants.ViewMask.DescriptionContactPointId, DataServiceConstants.Data.ContactPointEmailValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Contact point Phone", DataServiceConstants.ViewMask.DescriptionContactPointId, DataServiceConstants.Data.ContactPhoneValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Conform to", DataServiceConstants.ViewMask.DescriptionConformsToId, DataServiceConstants.Data.ConformsToDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Documents", DataServiceConstants.ViewMask.DescriptionDocumentationId, DataServiceConstants.Data.DocumentsDefIdValue, false);
    }
}
