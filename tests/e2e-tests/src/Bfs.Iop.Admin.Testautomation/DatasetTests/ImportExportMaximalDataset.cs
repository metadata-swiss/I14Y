using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;
using MyShare = Bfs.Iop.Admin.Testautomation.Shared;

namespace Bfs.Iop.Admin.Testautomation.DatasetTests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class ImportExportMaximalDataset : PlaywrightSetup
{
    private string _maximalTitleDataset;
    private string _maximalIdentifierDataset;
    private string _maximalTitleDistribution;
    private string _maximalIdentifierDistribution;

    private StandardTask? _standardAction;
    private Listener? _listener;
    private string _bearerToken = string.Empty;
    private readonly string PeriodDate = DateTime.Now.AddMonths(6).ToString("dd.MM.yyyy");

    private string _downloadPath;
    private string _uploadPath;

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

        var vocabularyValues = await DatasetsEditMaskHelper.FillDatasetMaximal(
            Actions, 
            _maximalTitleDataset, 
            _maximalIdentifierDataset, 
            CurrentDate, 
            PeriodDate);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataset(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");

        await CheckNewDataset(vocabularyValues);
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
    /// Switch to the view of the dataset, click on “Export” and download the dataset as JSON.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldExportDatasetSuccessfully()
    {
        var title = _maximalTitleDataset;

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, title);

        await Actions.ScrollIntoViewById(DatasetConstants.ViewMask.ExportButtonId);

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(DatasetConstants.ViewMask.ExportButtonId);

        await Actions.WaitForSpinnerToDisappear();

        _downloadPath = await Actions.ClickDownloadById(DatasetConstants.ViewMask.ExportJsonId);

        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear();

        await Actions.PressKey(Keys.Escape);

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
        _listener.ResetErrors();
    }

    [Test, Order(5)]
    public async Task ShouldDoubleCheckDownloadSuccessfully()
    {
        string content = await File.ReadAllTextAsync(_downloadPath);

        var options = CreateJsonSerializerOptions();

        var model = JsonSerializer.Deserialize<DataWrapper<DcatDatasetInputModel>>(content, options);

        if (model != null && model.Data != null)
        {
            DoubleCheckModel(model.Data);
        }
        else
        {
            TestContext.Out.WriteLine($"{nameof(DcatDatasetInputModel)} could not be deserialized");
            Assert.IsFalse(true);
        }
    }

    [Test, Order(6)]
    public async Task ShouldOverwriteJsonSuccessfully()
    {
        string content = await File.ReadAllTextAsync(_downloadPath);

        var options = CreateJsonSerializerOptions();

        var wrapper = JsonSerializer.Deserialize<DataWrapper<DcatDatasetInputModel>>(content, options);

        if (wrapper != null && wrapper.Data != null)
        {
            await StoreCopyFile(wrapper.Data);
        }
        else
        {
            TestContext.Out.WriteLine($"{nameof(DcatDatasetInputModel)} could not be deserialized");
            Assert.IsFalse(true);
        }
    }

    [Test, Order(7)]
    public async Task ShouldImportDatasetSuccessfully()
    {
        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenImportDatasetModalWindow(Actions);

        var inputFiles = await Actions.FindElementByCssSelector("input[aria-labelledby='ob-file-upload-caption']");

        Assert.That(inputFiles == null, Is.False, "Cannot reach upload input");
        
        await inputFiles!.SetInputFilesAsync(_uploadPath);

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
        _listener.ResetErrors();
    }

    /// <summary>
    /// Deletes the dataset including distribution via the API and checks that deletion was successful.
    /// </summary>
    [Test, Order(8)]
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

    private async Task CheckDatasetMaximal(string title, IReadOnlyDictionary<string, string> usedVocabularyValues)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Title", DatasetConstants.ViewMask.DatasetTitleId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Description", DatasetConstants.ViewMask.DatasetDescriptionId, DatasetConstants.Data.Description);

        await _standardAction!.CheckDivPropertyById(Actions, "Identifier", DatasetConstants.ViewMask.DatasetIdentifierId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Issue Date", DatasetConstants.ViewMask.DatasetPublicationDateId, CurrentDate);

        await _standardAction!.CheckDivPropertyById(Actions, "Publisher", DatasetConstants.ViewMask.DatasetPublisherId, DatasetConstants.Data.PublisherViewDe);

        await _standardAction!.CheckDivPropertyById(Actions, "Responsible Person", DatasetConstants.ViewMask.DatasetResponsiblePersonId, DatasetConstants.Data.ResponsiblePersonViewDe);

        await _standardAction!.CheckDivPropertyById(Actions, "Owner", DatasetConstants.ViewMask.DatasetOwnerId, DatasetConstants.Data.DataOwnerName);

        await _standardAction!.CheckDivPropertyById(Actions, "Languages", DatasetConstants.ViewMask.DatasetLanguagesId, usedVocabularyValues[DatasetConstants.EditMask.SelectLanguageId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Themes", DatasetConstants.ViewMask.DatasetThemesId, usedVocabularyValues[DatasetConstants.EditMask.SelectThemeCodesId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Period", DatasetConstants.ViewMask.DatasetPeriodId, PeriodDate);

        await _standardAction!.CheckDivPropertyById(Actions, "Landing pages", DatasetConstants.ViewMask.DatasetLandingPageId, DatasetConstants.Data.LandingPagesDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Access rights", DatasetConstants.ViewMask.DatasetAccessRightsId, usedVocabularyValues[DatasetConstants.EditMask.AccessRightsId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Spatial", DatasetConstants.ViewMask.DatasetSpatialId, DatasetConstants.Data.SpatialIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Geo", DatasetConstants.ViewMask.DatasetGeoId, usedVocabularyValues[DatasetConstants.EditMask.SelectGeoIvId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Coverage", DatasetConstants.ViewMask.DatasetCoverageId, CurrentDate, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Frequency", DatasetConstants.ViewMask.DistributionsFrequencyId, usedVocabularyValues[DatasetConstants.EditMask.SelectFrequencyId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Conforms to", DatasetConstants.ViewMask.DatasetConformsToId, DatasetConstants.Data.ConformsToDeIdValue, false);

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
        await _standardAction!.CheckDivPropertyById(Actions, "Media Type", DatasetConstants.ViewMask.DistributionsMediaTypeId, vocabularyValues[DatasetConstants.EditMask.DistributionMediaTypeId]);

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

        await _standardAction!.CheckDivPropertyById(Actions, "Conforms To", DatasetConstants.ViewMask.DistributionsConformsToId, DatasetConstants.Data.ConformsToDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Documentation", DatasetConstants.ViewMask.DistributionsDocumentationId, DatasetConstants.Data.DocumentsDefIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Access Services", DatasetConstants.ViewMask.DistributionsAccessServicesId, DatasetConstants.Data.NoneView);

        await _standardAction!.CheckDivPropertyById(Actions, "Image", DatasetConstants.ViewMask.DistributionsImageId, DatasetConstants.Data.ImageDefIdValue, false);
    }

    private void DoubleCheckModel(DcatDatasetInputModel model)
    {
        TestContext.Out.WriteLine("Starting dataset Validation");

        Assert.That(model.Title, Is.Not.Null, "Title should not be null");
        Assert.IsNotNull(model.Title.De, "Title.De should not be null");
        Assert.That(model.Title.De, Is.EqualTo(_maximalTitleDataset));

        Assert.IsNotNull(model.Description, "Description should not be null");
        Assert.IsNotNull(model.Description.De, "Description.De should not be null");
        Assert.IsTrue(model.Description.De.Contains(DatasetConstants.Data.Description), "Description does not match");

        Assert.IsNotNull(model.Identifiers, "Identifiers should not be null");
        Assert.That(model.Identifiers.Count(), Is.EqualTo(1), "Identifiers should contain exactly 1 element");
        Assert.That(model.Identifiers.First(), Is.EqualTo(_maximalIdentifierDataset), "Identifiers does not match");

        Assert.IsNotNull(model.Issued, "Issued should not be null");
        Assert.That(model.Issued.Value.Date, Is.EqualTo(DateTime.Parse(CurrentDate)), "Issued date does not match");

        Assert.IsNotNull(model.Publisher, "Publisher should not be null");
        Assert.That(model.Publisher.Identifier, Is.EqualTo("i14y-test-organisation"));

        Assert.IsNotNull(model.Languages, "Languages should not be null");
        Assert.That(model.Languages.Count(), Is.EqualTo(3), "Languages should contain 3 elements");

        Assert.IsNotNull(model.AccessRights, "AccessRights should not be null");

        Assert.IsNotNull(model.GeoIvIds, "GeoIvIds should not be null");
        Assert.That(model.GeoIvIds.Count(), Is.EqualTo(1), "GeoIvIds should contain 1 element");

        Assert.IsNull(model.Frequency, "Frequency should be null");

        Assert.IsNotNull(model.ConformsTo, "ConformsTo should not be null");
        Assert.That(model.ConformsTo.Count(), Is.EqualTo(1), "ConformsTo should contain 1 element");
        Assert.That(model.ConformsTo.First().Label!.De, Is.EqualTo(DatasetConstants.Data.ConformsToDeIdValue));
        Assert.That(model.ConformsTo.First().Uri, Is.EqualTo(DatasetConstants.Data.ConformsToHrefIdValue));

        Assert.IsNotNull(model.Spatial, "Spatial should not be null");
        Assert.That(model.Spatial.Count(), Is.EqualTo(1), "Spatial should contain 1 element");
        Assert.IsTrue(DatasetConstants.Data.SpatialIdValue.Contains(model.Spatial.First()));

        Assert.IsNotNull(model.TemporalCoverage, "TemporalCoverage should not be null");
        Assert.That(model.TemporalCoverage.Count(), Is.EqualTo(1), "TemporalCoverage should contain 1 element");
        Assert.IsNotNull(model.TemporalCoverage.First().Start, "Start date should not be null");
        Assert.That(model.TemporalCoverage.First().Start.Value.Date, Is.EqualTo(DateTime.Parse(CurrentDate)), "Start date does not match");

        Assert.IsNotNull(model.Keywords, "Keywords should not be null");
        Assert.That(model.Keywords.Count(), Is.EqualTo(1), "Keywords should contain 1 element");
        Assert.That(model.Keywords.First()!.Label!.De, Is.EqualTo(DatasetConstants.Data.KeywordsDeValue));

        Assert.IsNotNull(model.LandingPages, "LandingPages should not be null");
        Assert.That(model.LandingPages.Count(), Is.EqualTo(1), "LandingPages should contain 1 element");
        Assert.That(model.LandingPages.First().Label!.De, Is.EqualTo(DatasetConstants.Data.LandingPagesDeIdValue));
        Assert.That(model.LandingPages.First().Uri, Is.EqualTo(DatasetConstants.Data.LandingPagesHrefValue));

        Assert.IsNotNull(model.IsReferencedBy, "IsReferencedBy should not be null");
        Assert.That(model.IsReferencedBy.Count(), Is.EqualTo(1), "IsReferencedBy should contain 1 element");
        Assert.That(model.IsReferencedBy.First().Label!.De, Is.EqualTo(DatasetConstants.Data.IsReferencedByDeIdValue));
        Assert.That(model.IsReferencedBy.First().Uri, Is.EqualTo(DatasetConstants.Data.IsReferencedByHrefIdValue));

        Assert.IsNotNull(model.Distributions, "Distributions should not be null");
        Assert.That(model.Distributions.Count(), Is.EqualTo(1), "Distributions should contain 1 element");
        var dist = model.Distributions.First();

        Assert.That(dist.Identifier, Is.EqualTo(_maximalTitleDistribution));
        Assert.That(dist.Title.De, Is.EqualTo(_maximalTitleDistribution));
        Assert.That(dist.Description.De!.StartsWith(_maximalTitleDistribution), Is.True, "Distribution Description does not have the expected content");
        Assert.That(dist.AccessUrl.Uri, Is.EqualTo(DatasetConstants.Data.AccessUrlDistributionIdValue));
        Assert.That(dist.AccessUrl.Label!.De, Is.EqualTo("i14y"));
        Assert.That(dist.DownloadUrl!.Uri, Is.EqualTo(DatasetConstants.Data.AccessUrlDistributionIdValue));
        Assert.That(dist.ByteSize, Is.EqualTo(1024.0));
        Assert.That(dist.Rights, Is.EqualTo("Freiheitsrechte, Sozialrechte, Kollektivrechte"));
        Assert.That(dist.TemporalResolution, Is.EqualTo(DatasetConstants.Data.TemporalResolutionDistributionIdValue));

        Assert.IsNotNull(dist.Checksum, "Distribution Checksum should not be null");
        Assert.That(dist.Checksum.ChecksumValue, Is.EqualTo("1234567890"));
        Assert.That(dist.Checksum.Algorithm, Is.Not.Null, "Distribution Checksum algorithm should not be null");

        Assert.IsNotNull(dist.License, "Distribution License should not be null");

        Assert.IsNotNull(dist.Languages, "Distribution Languages should not be null");
        Assert.That(dist.Languages.Count(), Is.EqualTo(3), "Distribution Languages should contain 3 elements");

        Assert.IsNotNull(model.Themes, "Themes should not be null");
        Assert.That(model.Themes.Count(), Is.EqualTo(3), "Themes should contain 3 elements");

        Assert.IsNotNull(model.ResponsiblePerson, "ResponsiblePerson should not be null");
        Assert.That(model.ResponsiblePerson.Email, Is.EqualTo(DatasetConstants.Data.ResponsiblePersonName));

        Assert.That(model.DataOwner, Is.EqualTo(DatasetConstants.Data.DataOwnerName));

        Assert.IsNotNull(model.RetentionPeriod, "RetentionPeriod should not be null");
        Assert.That(model.RetentionPeriod.Value.Date, Is.EqualTo(DateTime.Parse(PeriodDate)), "RetentionPeriod does not match");

        TestContext.Out.WriteLine("✓ All entries validated successfully");
    }

    private async Task StoreCopyFile(DcatDatasetInputModel model)
    {
        var copy = model with
        {
            Identifiers = [.. model.Identifiers.Select((id, index) => index == 0 ? id + "copy" : id)]
        };

        var wrapper = new DataWrapper<DcatDatasetInputModel>(copy);

        string directory = Path.GetDirectoryName(_downloadPath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_downloadPath);
        string extension = Path.GetExtension(_downloadPath);
        _uploadPath = Path.Combine(directory, fileNameWithoutExtension + "_copy" + extension);

        var writeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string modifiedContent = JsonSerializer.Serialize(wrapper, writeOptions);
        await File.WriteAllTextAsync(_uploadPath, modifiedContent);

        TestContext.Out.WriteLine($"Modified file saved to: {_uploadPath}");
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
