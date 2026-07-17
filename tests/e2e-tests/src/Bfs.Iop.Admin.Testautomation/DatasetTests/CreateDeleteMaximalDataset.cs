using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;
using MyShare = Bfs.Iop.Admin.Testautomation.Shared;

namespace Bfs.Iop.Admin.Testautomation.DatasetTests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class CreateDeleteMaximalDataset : PlaywrightSetup
{
    private string _maximalTitleDataset;
    private string _maximalIdentifierDataset;
    private string _maximalTitleDistribution;
    private string _maximalIdentifierDistribution;

    private StandardTask? _standardAction;
    private Listener? _listener;
    private string _bearerToken = string.Empty;
    private readonly string PeriodDate = DateTime.Now.AddMonths(6).ToString("dd.MM.yyyy");

    [SetUp]
    public void SetupInternal()
    {
        _maximalTitleDataset = DatasetConstants.Data.TitleDataset + TimeStamp;
        _maximalIdentifierDataset = DatasetConstants.Data.IdentifierDataset + TimeStamp;
        _maximalTitleDistribution = DatasetConstants.Data.TitleDistribution + TimeStamp;
        _maximalIdentifierDistribution = DatasetConstants.Data.IdentifierDistribution + TimeStamp;
        
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

    /// <summary>
    /// Creates a new dataset with maximum data and checks that it has been successfully created and displayed.
    /// </summary>
    [Test, Order(1)]
    public async Task ShouldCreateNewDatasetWhenProvidingMaximalDataSuccessfully()
    {
        await _standardAction!.ChangeLanguageToGerman(Actions);

        TestContext.Out.WriteLine($"Go to: {BaseAdminUrl + AttributesAndElements.Home}");

        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenCreateDatasetMask(Actions);

        var usedVocabularyValues = await DatasetsEditMaskHelper.FillDatasetMaximal(
            Actions,
            _maximalTitleDataset,
            _maximalIdentifierDataset, 
            CurrentDate,
            PeriodDate);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataset(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");

        await CheckNewDataset(usedVocabularyValues);
    }

    /// <summary>
    /// Creates a distribution for the existing dataset with maximum data and checks that it has been created successfully.
    /// </summary>
    [Test, Order(2)]
    public async Task ShouldCreateDistributionWhenDatasetExistsSuccessfully()
    {
        var title = _maximalTitleDataset;
        await _standardAction!.ChangeLanguageToGerman(Actions);

        TestContext.Out.WriteLine("Start create a distribution for the generated dataset");

        await _standardAction!.GotoCatalog(Actions);

        await Actions.Wait1000();

        await _standardAction!.OpenDistributionCreateMaskByDatasetName(Actions, title);

        var vocabularyValues = await DatasetsEditMaskHelper.FillCreateDistributionMaximal(Actions, _maximalTitleDistribution, _maximalIdentifierDistribution, CurrentDate);

        await Actions.Wait1000();

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDistribution(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");

        await CheckNewDistribution(title, vocabularyValues);
    }

    /// <summary>
    /// Deletes the dataset including distribution via the API and checks that deletion was successful.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldDeleteDatasetIncludingDistributionSuccessfully()
    {
        var title = _maximalTitleDataset;

        try
        {
            await Page.GotoAsync(BaseAdminUrl);

            await Actions.Wait500();

            if (string.IsNullOrWhiteSpace(_bearerToken))
            {
                _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);
            }

            var deleteTests = new MyShare.DeleteTests(BaseIopCoreUrl, _bearerToken, title);

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

    private async Task CheckNewDataset(IReadOnlyDictionary<string, string> usedVocabularyValues)
    {
        var title = _maximalTitleDataset;

        TestContext.Out.WriteLine($"Check the contents of the new dataset {title}");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, title);

        await CheckDatasetMaximal(title, usedVocabularyValues);
    }

    private async Task CheckDatasetMaximal(string title, IReadOnlyDictionary<string, string> vocabularyValues)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Title", DatasetConstants.ViewMask.DatasetTitleId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Description", DatasetConstants.ViewMask.DatasetDescriptionId, DatasetConstants.Data.Description);

        await _standardAction!.CheckDivPropertyById(Actions, "Identifier", DatasetConstants.ViewMask.DatasetIdentifierId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Issue Date", DatasetConstants.ViewMask.DatasetPublicationDateId, CurrentDate);

        await _standardAction!.CheckDivPropertyById(Actions, "Publisher", DatasetConstants.ViewMask.DatasetPublisherId, DatasetConstants.Data.PublisherViewDe);

        await _standardAction!.CheckDivPropertyById(Actions, "Responsible Person", DatasetConstants.ViewMask.DatasetResponsiblePersonId, DatasetConstants.Data.ResponsiblePersonViewDe);

        await _standardAction!.CheckDivPropertyById(Actions, "Owner", DatasetConstants.ViewMask.DatasetOwnerId, DatasetConstants.Data.DataOwnerName);

        await _standardAction!.CheckDivPropertyById(Actions, "Languages", DatasetConstants.ViewMask.DatasetLanguagesId, vocabularyValues[DatasetConstants.EditMask.SelectLanguageId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Themes", DatasetConstants.ViewMask.DatasetThemesId, vocabularyValues[DatasetConstants.EditMask.SelectThemeCodesId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Period", DatasetConstants.ViewMask.DatasetPeriodId, PeriodDate);

        await _standardAction!.CheckDivPropertyById(Actions, "Landing pages", DatasetConstants.ViewMask.DatasetLandingPageId, DatasetConstants.Data.LandingPagesDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Access rights", DatasetConstants.ViewMask.DatasetAccessRightsId, vocabularyValues[DatasetConstants.EditMask.AccessRightsId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Spatial", DatasetConstants.ViewMask.DatasetSpatialId, DatasetConstants.Data.SpatialIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Geo", DatasetConstants.ViewMask.DatasetGeoId, vocabularyValues[DatasetConstants.EditMask.SelectGeoIvId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Coverage", DatasetConstants.ViewMask.DatasetCoverageId, CurrentDate, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Frequency", DatasetConstants.ViewMask.DistributionsFrequencyId, vocabularyValues[DatasetConstants.EditMask.SelectFrequencyId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Conform to", DatasetConstants.ViewMask.DatasetConformsToId, DatasetConstants.Data.ConformsToDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Referenced by", DatasetConstants.ViewMask.DatasetReferencedById, DatasetConstants.Data.IsReferencedByDeIdValue, false);
    }

    private async Task CheckNewDistribution(string title, IReadOnlyDictionary<string, string> vocabularyValues)
    {
        TestContext.Out.WriteLine("Check new distribution");
        await _standardAction!.GotoCatalog(Actions);
        await Actions.Wait500();
        await _standardAction!.OpenDistributionViewByDatasetName(Actions, title);

        await CheckDistributionMaximal(vocabularyValues);
    }

    private async Task CheckDistributionMaximal(IReadOnlyDictionary<string, string> vocabularyValues)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Title", DatasetConstants.ViewMask.DistributionsTitleId, _maximalTitleDistribution);

        await _standardAction!.CheckDivPropertyById(Actions, "Description", DatasetConstants.ViewMask.DistributionsDescriptionId, _maximalTitleDistribution);

        await _standardAction!.CheckDivPropertyById(Actions, "Identifier", DatasetConstants.ViewMask.DistributionsIdentifierId, _maximalIdentifierDistribution);

        await _standardAction!.CheckDivPropertyById(Actions, "Release Date", DatasetConstants.ViewMask.DistributionsReleaseDateId, CurrentDate);

        await _standardAction!.CheckDivPropertyById(Actions, "Modified Date", DatasetConstants.ViewMask.DistributionsModifiedId, DatasetConstants.Data.NoneView);

        await _standardAction!.CheckDivPropertyById(Actions, "Language", DatasetConstants.ViewMask.DistributionsLanguageId, vocabularyValues[DatasetConstants.EditMask.DistributionLanguageId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Access Url", DatasetConstants.ViewMask.DistributionsAccessUrlId, DatasetConstants.Data.AccessUrlTitleDeDistributionIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Download Url", DatasetConstants.ViewMask.DistributionsDownloadUrlId, DatasetConstants.Data.AccessUrlTitleDeDistributionIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Size", DatasetConstants.ViewMask.DistributionsSizeId, DatasetConstants.Data.SizeViewDe);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "Format", DatasetConstants.ViewMask.DistributionsFormatId, vocabularyValues[DatasetConstants.EditMask.DistributionFormatId]);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "Mediatype", DatasetConstants.ViewMask.DistributionsMediaTypeId, vocabularyValues[DatasetConstants.EditMask.DistributionMediaTypeId]);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "Packaging Format", DatasetConstants.ViewMask.DistributionsPackagingFormatId, vocabularyValues[DatasetConstants.EditMask.DistributionPackagingFormatId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Checksum", DatasetConstants.ViewMask.DistributionsChecksumId, DatasetConstants.Data.ChecksumViewDe);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "License", DatasetConstants.ViewMask.DistributionsLicenseId, vocabularyValues[DatasetConstants.EditMask.DistributionLicenseId]);
       
        await _standardAction!.CheckDivPropertyById(Actions, "Rights", DatasetConstants.ViewMask.DistributionsRightsId, DatasetConstants.Data.RightsDistributionIdValue);

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        await _standardAction!.CheckDivPropertyById(Actions, "Availability", DatasetConstants.ViewMask.DistributionsAvailabilityId, vocabularyValues[DatasetConstants.EditMask.DistributionAvailabilityId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Coverage From", DatasetConstants.ViewMask.DistributionsCoverageFromId, CurrentDate);

        await _standardAction!.CheckDivPropertyById(Actions, "Coverage To", DatasetConstants.ViewMask.DistributionsCoverageToId, DatasetConstants.Data.NoneView);

        await _standardAction!.CheckDivPropertyById(Actions, "Temporal Resolution", DatasetConstants.ViewMask.DistributionsTemporalResolutionId, DatasetConstants.Data.TemporalResolutionDistributionIdValue);

        await _standardAction!.CheckDivPropertyById(Actions, "Conform To", DatasetConstants.ViewMask.DistributionsConformsToId, DatasetConstants.Data.ConformsToDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Documentation", DatasetConstants.ViewMask.DistributionsDocumentationId, DatasetConstants.Data.DocumentsDefIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Access Services", DatasetConstants.ViewMask.DistributionsAccessServicesId, DatasetConstants.Data.NoneView);

        await _standardAction!.CheckDivPropertyById(Actions, "Image", DatasetConstants.ViewMask.DistributionsImageId, DatasetConstants.Data.ImageDefIdValue, false);


    }

}
