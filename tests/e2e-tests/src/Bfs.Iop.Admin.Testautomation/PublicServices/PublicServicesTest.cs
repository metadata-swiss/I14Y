using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Enums;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;
using Dialogs = Bfs.Iop.Admin.Testautomation.Constants.Dialogs;
using MyShare = Bfs.Iop.Admin.Testautomation.Shared;

namespace Bfs.Iop.Admin.Testautomation.PublicServices;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class PublicServicesTest : PlaywrightSetup
{
    private const string _secondPrefix = "Second";

    private string _maximalTitlePublicService;

    private string _minimalTitleDataset;
    private string _minimalIdentifierDataset;

    private StandardTask? _standardAction;
    private Listener? _listener;
    private string _bearerToken = string.Empty;

    [SetUp]
    public void SetupInternal()
    {
        _maximalTitlePublicService = PublicServiceConstants.Data.TitlePublicServices + TimeStamp;
        _minimalTitleDataset = DatasetConstants.Data.TitleDataset + TimeStamp;
        _minimalIdentifierDataset = DatasetConstants.Data.IdentifierDataset + TimeStamp;

        _standardAction = new MyShare.StandardTask();
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

    // <summary>
    /// Creates two public services with complete data and checks their setup.
    /// </summary>
    [Test, Order(1)]
    public async Task ShouldCreateNewPublicServicesWhenProvidingMaximalDataSuccessfully()
    {
        var title = _maximalTitlePublicService;

        TestContext.Out.WriteLine($"Start create a PublicService : {title}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        await _standardAction!.GotoCatalog(Actions);

        var usedVocabularyValues = await CreatePublicService(title);

        await _standardAction!.GotoCatalog(Actions);

        await CreatePublicService(title + _secondPrefix);

        await CheckPublicService(title, usedVocabularyValues);
    }

    /// <summary>
    /// Creates a dataset that is linked in the public service and checks the attachment.
    /// </summary>
    [Test, Order(2)]
    public async Task ShouldCreateDatasetForLinkSuccessfully()
    {
        var title = _minimalTitleDataset;

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenCreateDatasetMask(Actions);

        await DatasetsEditMaskHelper.FillDatasetMinimal(Actions, title, _minimalIdentifierDataset, CurrentDate);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataset(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        AssertNoApiErrors();
    }

    /// <summary>
    /// Links the dataset and the second public service in the first entry and checks the links.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldLinkDatasetAndSecondPublicServiceInPublicServicesSuccessfully()
    {
        var title = _maximalTitlePublicService;
        var titleDataset = _minimalTitleDataset;

        TestContext.Out.WriteLine($"Set links in PublicService : {title}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await Actions.WaitForSpinnerToDisappear();

        await _standardAction!.OpenEditPublicServicesMaskByName(Actions, title);

        await PublicServiceEditMaskHelper.FillPublicServiceLinks(Actions, title + _secondPrefix, titleDataset);

        await _standardAction!.SaveAndClosePublicService(Actions);

        await CheckLinksInPublicService(title, titleDataset);
    }

    [Test, Order(4)]
    public async Task ShouldCreateChannelsSuccessfully()
    {
        TestContext.Out.WriteLine($"Create Channels for PublicServices : {_maximalTitlePublicService}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await Actions.WaitForSpinnerToDisappear();

        await _standardAction!.OpenEditPublicServicesMaskByName(Actions, _maximalTitlePublicService);

        await CreateChannels();

        await _standardAction!.SaveAndClosePublicService(Actions);
    }

    /// <summary>
    /// Sets and resets the status and level of public service and checks the chips.
    /// </summary>
    [Test, Order(5)]
    public async Task ShouldSetAndResetPublicServiceStatusAndLevelSuccessfully()
    {
        var title = _maximalTitlePublicService;

        TestContext.Out.WriteLine($"Set and Reset PublicService Status and Level: {title}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewPublicServicesMaskByName(Actions, title);

        await _standardAction!.SetPublicLevelToPublic(Actions);
        AssertNoApiErrors();

        var status = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipStatusId);
        var level = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipLevelId);
        Assert.That(status, Is.EqualTo(StatusLevelVersion.RecordedNameDe), $"status is not set correctly. {StatusLevelVersion.RecordedNameDe} instead of {status}.");
        Assert.That(level.Equals(StatusLevelVersion.PublicNameDe), Is.True, $"level is not set correctly. {StatusLevelVersion.PublicNameDe} instead of {level}.");

        await _standardAction!.ResetPublicLevelToInternal(Actions);
        AssertNoApiErrors();

        var status2 = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipStatusId);
        var level2 = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipLevelId);
        Assert.That(status2.Equals(StatusLevelVersion.IncompleteNameDe), Is.True, $"status is not set correctly. {StatusLevelVersion.IncompleteNameDe} instead of {status2}.");
        Assert.That(level2.Equals(StatusLevelVersion.UnitNameDe), Is.True, $"level is not set correctly. {StatusLevelVersion.UnitNameDe} instead of {level2}.");
    }

    /// <summary>
    /// Removes the linked dataset and checks the unlinking in Public Service.
    /// </summary>
    [Test, Order(6)]
    public async Task ShouldUnlinkDatasetSuccessfully()
    {
        var title = _maximalTitlePublicService;

        TestContext.Out.WriteLine($"remove links in PublicService : {title}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await Actions.WaitForSpinnerToDisappear();

        await _standardAction!.OpenEditPublicServicesMaskByName(Actions, title);

        await PublicServiceEditMaskHelper.RemovePublicServiceLinks(Actions);

        await _standardAction!.SaveAndClosePublicService(Actions);

        await CheckUnLinksInPublicService(title);
    }

    /// <summary>
    /// Removes all channels and checks that they have been removed.
    /// </summary>
    [Test, Order(7)]
    public async Task ShouldDeleteAllChannelsSuccessfully()
    {
        var title = _maximalTitlePublicService;

        TestContext.Out.WriteLine($"Open PublicService : {title}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await Actions.WaitForSpinnerToDisappear();

        await _standardAction!.OpenEditPublicServicesMaskByName(Actions, title);

        TestContext.Out.Write("Deleting all Channels");

        await Actions.ScrollOnTopById(PublicServiceConstants.EditMask.ChannelTableId);

        // The checkbox is nested in the table
        var selectAllCheckbox = await Actions.FindNestedElement(PublicServiceConstants.EditMask.ChannelTableId, "mdc-checkbox__native-control");
        await selectAllCheckbox!.ClickAsync();

        await Actions.ClickButtonById(PublicServiceConstants.EditMask.ChannelTableDeleteAllButtonId);

        var deleteAllButton = await Actions.FindElementById(PublicServiceConstants.EditMask.ChannelTableDeleteAllButtonId);
        bool isButtonEnabled = !await Actions.IsDisabled(deleteAllButton);

        Assert.That(isButtonEnabled, Is.True, "The delete all channels button is not enabled.");

        await Actions.ClickButtonById(PublicServiceConstants.EditMask.ChannelTableConfirmDeleteDialogButtonId);

        // ToDo: Check that all have been deleted
        await Actions.WaitForSpinnerToDisappear();

        await _standardAction!.SaveAndClosePublicService(Actions);
    }

    /// <summary>
    /// Deletes the previously linked dataset via API and checks whether the operation was successful.
    /// </summary>
    [Test, Order(8)]
    public async Task ShouldDeleteDatasetSuccessfully()
    {
        var titleDataset = _minimalTitleDataset;

        try
        {
            await Page.GotoAsync(BaseAdminUrl);

            await Actions.Wait500();

            if (string.IsNullOrWhiteSpace(_bearerToken))
            {
                _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);
            }

            var deleteTests = new MyShare.DeleteTests(BaseIopCoreUrl, _bearerToken, titleDataset);

            var result = await deleteTests.CleanupDataset();

            Assert.That(result.Success, Is.True, result.Message);
            TestContext.Out.WriteLine("Delete test completed");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine(ex.Message);
            Assert.Fail($"Error delete items: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes both public services created and checks that they have been deleted successfully.
    /// </summary>
    /// <returns></returns>
    [Test, Order(9)]
    public async Task ShouldDeletePublicServicesSuccessfully()
    {
        var publicServiceTitle = _maximalTitlePublicService;
        var secondPublicServiceTitle = publicServiceTitle + _secondPrefix;

        TestContext.Out.Write($"Delete PublicService via api: '{secondPublicServiceTitle}");

        var deleteTests = new DeleteTests(BaseIopCoreUrl, _bearerToken, secondPublicServiceTitle);

        await deleteTests.DeletePublicService();

        TestContext.Out.WriteLine($"Delete PublicService '{publicServiceTitle}'");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewPublicServicesMaskByName(Actions, publicServiceTitle);

        var deleteButton = await Actions.FindElementById(PublicServiceConstants.ViewMask.PublicServiceViewDeleteButtonId);
        bool isEnabled = !await Actions.IsDisabled(deleteButton);

        if (isEnabled)
        {
            await DeletePublicService();
        }

        await Actions.WaitForSpinnerToDisappear();
    }

    private async Task CheckPublicService(string title, IReadOnlyDictionary<string, string> usedVocabularyValues)
    {
        TestContext.Out.WriteLine($"Check the contents of the new public service {title}");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.SearchPublicServicesByName(Actions, title);

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(PublicServiceConstants.ViewMask.CatalogTableViewButton + "0");
        await Actions.WaitForSpinnerToDisappear();

        await CheckPublicServiceProperties(title, usedVocabularyValues);
    }

    private async Task CheckLinksInPublicService(string title, string titleDataset)
    {
        TestContext.Out.WriteLine($"Check the contents of the new public service {title}");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.SearchPublicServicesByName(Actions, title);

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(PublicServiceConstants.ViewMask.CatalogTableViewButton + "0");

        await Actions.WaitForSpinnerToDisappear();

        await CheckLinksPublicServiceProperties(title, titleDataset);
    }


    private async Task CheckPublicServiceProperties(string title, IReadOnlyDictionary<string, string> usedVocabularyValues)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Title", PublicServiceConstants.ViewMask.PublicServiceViewTitleId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Description", PublicServiceConstants.ViewMask.PublicServiceViewDescriptionId, PublicServiceConstants.Data.Description);

        await _standardAction!.CheckDivPropertyById(Actions, "Identifier", PublicServiceConstants.ViewMask.PublicServiceViewIdentifierId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Publisher", PublicServiceConstants.ViewMask.PublicServiceViewPublisherId, PublicServiceConstants.Data.PublisherViewDe);

        await _standardAction!.CheckDivPropertyById(Actions, "Themes", PublicServiceConstants.ViewMask.PublicServiceViewThemesId, usedVocabularyValues[PublicServiceConstants.EditMask.PublicServiceThemesId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Keywords", PublicServiceConstants.ViewMask.PublicServiceViewKeywordsId, PublicServiceConstants.Data.KeywordsDeValue);

        await _standardAction!.CheckDivPropertyById(Actions, "Sector", PublicServiceConstants.ViewMask.PublicServiceViewSectorId, usedVocabularyValues[PublicServiceConstants.EditMask.PublicServiceDescriptionSectorId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Languages", PublicServiceConstants.ViewMask.PublicServiceViewLanguagesId, usedVocabularyValues[PublicServiceConstants.EditMask.PublicServiceLanguagesId]);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "Business Events", PublicServiceConstants.ViewMask.PublicServiceViewBusinessEventsId, usedVocabularyValues[PublicServiceConstants.EditMask.PublicServiceDescriptionBusinessEventsId]);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "Live Events", PublicServiceConstants.ViewMask.PublicServiceViewLifeEventsId, usedVocabularyValues[PublicServiceConstants.EditMask.PublicServiceDescriptionLifeEventsId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Spatial CH", PublicServiceConstants.ViewMask.PublicServiceViewSpatialChId, usedVocabularyValues[PublicServiceConstants.EditMask.FormatSpatialChId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Spatial", PublicServiceConstants.ViewMask.PublicServiceViewSpatialId, PublicServiceConstants.Data.SpatialDeValue);
    }

    private async Task CheckLinksPublicServiceProperties(string title, string titleDataset)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Requires", PublicServiceConstants.ViewMask.PublicServiceRequiresId + "0", title + _secondPrefix, false);

        await _standardAction!.CheckDivPropertyById(Actions, "LinkWith", PublicServiceConstants.ViewMask.PublicServiceRelationId + "0", title + _secondPrefix, false);

        await _standardAction!.CheckDivPropertyById(Actions, "IsDescribed", PublicServiceConstants.ViewMask.PublicServiceDescribedId + "0", titleDataset, false);
    }

    private async Task DeletePublicService()
    {
        await Actions.ClickButtonById(PublicServiceConstants.ViewMask.PublicServiceViewDeleteButtonId);

        await Actions.WaitForSpinnerToDisappear();

        var urlTracker = new PageUrlTracker(Page);

        await Actions.ClickButtonById(Dialogs.ConfirmId);
        await Actions.Wait1000();

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");
        AssertNoApiErrors();

        await Actions.WaitForSpinnerToDisappear();
    }

    private void AssertNoApiErrors()
    {
        Assert.That(_listener!.HasApiErrors(), Is.False,
            $"API errors occurred during test execution:\n{_listener.GetLastErrorMessage()}");
    }

    private async Task<IReadOnlyDictionary<string, string>> CreatePublicService(string title)
    {
        await _standardAction!.OpenCreatePublicServicesMask(Actions);

        var values = await PublicServiceEditMaskHelper.FillPublicServiceMaximal(Actions, title);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndClosePublicService(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        AssertNoApiErrors();

        return values;
    }

    private static string ConvertChannelTypeToGerman(ChannelType type) => type switch
    {
        ChannelType.Post => PublicServiceConstants.Data.ChannelViewTypePostDE,
        ChannelType.Email => PublicServiceConstants.Data.ChannelViewTypeEmailDE,
        ChannelType.Mobile => PublicServiceConstants.Data.ChannelViewTypeMobileDE,
        ChannelType.Fax => PublicServiceConstants.Data.ChannelViewTypeFaxDE,
        ChannelType.Internet => PublicServiceConstants.Data.ChannelViewTypeInternetDE,
        ChannelType.Phone => PublicServiceConstants.Data.ChannelViewTypePhoneDE,
        _ => throw new NotSupportedException($"Unknown type: {type}."),
    };

    private async Task CheckChannelCreated(int index, string channelTitle, ChannelType channelType)
    {
        TestContext.Out.WriteLine($"Checking creation of the channel of the type '{channelType}'");

        await Actions.ScrollOnTopById(PublicServiceConstants.EditMask.ChannelTableId);

        await _standardAction!.CheckDivPropertyById(Actions, "Channel name", PublicServiceConstants.ViewMask.ChannelTableIdentifierId + index, channelTitle, true);

        await _standardAction!.CheckDivPropertyById(Actions, "Channel type", PublicServiceConstants.ViewMask.ChannelTableTypeId + index, ConvertChannelTypeToGerman(channelType), true);
    }

    private async Task CreateChannels()
    {
        TestContext.Out.WriteLine("Create public service channels");

        var channelTypes = Enum.GetValues<ChannelType>();

        foreach (var channelType in channelTypes)
        {
            TestContext.Out.WriteLine($"Create public service channel of the type '{channelType}'");

            await Actions.ScrollOnTopById(PublicServiceConstants.EditMask.ChannelTableId);

            await Actions.ClickButtonById(PublicServiceConstants.EditMask.ChannelTableAddRowButtonId);

            await PublicServiceEditMaskHelper.FillCreateChannelMask(Actions, _maximalTitlePublicService + channelType, channelType);

            await Actions.ClickButtonById(PublicServiceConstants.EditMask.ChannelSaveAndCloseId2);

            await Actions.WaitForSpinnerToDisappear();

            await CheckChannelCreated(Array.IndexOf(channelTypes, channelType), _maximalTitlePublicService + channelType, channelType);
        }
    }

    private async Task CheckUnLinksInPublicService(string title)
    {
        TestContext.Out.WriteLine($"Check the contents of the new public service {title}");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.SearchPublicServicesByName(Actions, title);

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(PublicServiceConstants.ViewMask.CatalogTableViewButton + "0");

        await Actions.WaitForSpinnerToDisappear();

        await CheckUnLinksPublicServiceProperties();
    }

    private async Task CheckUnLinksPublicServiceProperties()
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Requires", PublicServiceConstants.ViewMask.PublicServiceViewRequiresId, PublicServiceConstants.Data.NoneView);

        await _standardAction!.CheckDivPropertyById(Actions, "LinkWith", PublicServiceConstants.ViewMask.PublicServiceViewRelationId, PublicServiceConstants.Data.NoneView);

        await _standardAction!.CheckDivPropertyById(Actions, "IsDescribed", PublicServiceConstants.ViewMask.PublicServiceViewDescribedId, PublicServiceConstants.Data.NoneView);
    }
}
