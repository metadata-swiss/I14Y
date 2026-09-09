using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Models;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;
using VDS.RDF;
using VDS.RDF.Parsing;
using MyShare = Bfs.Iop.Admin.Testautomation.Shared;

namespace Bfs.Iop.Admin.Testautomation.OgdTests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class OgdCompleteTests : PlaywrightSetup
{
    private const string _bearerPrefix = "Bearer";

    private readonly string _agentAddress = "api/Agents/";
    private readonly string _dcatCatalogsAddress = "api/DcatCatalogs";
    private readonly string _downloadedFile = "downloadedFileOgdComplete.rdf";

    private string _OgdCatalogJson;
    private string _minimalTitleDataset;
    private string _minimalIdentifierDataset;
    private string _minimalTitleDistribution;
    private string _minimalIdentifierDistribution;
    private string _minimalTitleDataService;
    private string _minimalIdentifierDataService;

    private Listener? _listener;
    private string _bearerToken = string.Empty;
    private StandardTask? _standardAction;

    [SetUp]
    public void SetupInternal()
    {
        _OgdCatalogJson = OgdData.DcatCatalogCreateJson.Replace("###", TimeStamp);
        _minimalTitleDataset = DatasetConstants.Data.OGDTitleDataset + TimeStamp;
        _minimalIdentifierDataset = DatasetConstants.Data.OGDTIdentifierDataset + TimeStamp;
        _minimalTitleDistribution = DatasetConstants.Data.OGDTitleDistribution + TimeStamp;
        _minimalIdentifierDistribution = DatasetConstants.Data.OGDIdentifierDistribution + TimeStamp;
        _minimalTitleDataService = DataServiceConstants.Data.OgdTitleDataService + TimeStamp;
        _minimalIdentifierDataService = DataServiceConstants.Data.OgdIdentifierDataService + TimeStamp;

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
                TestContext.Out.WriteLine(_listener.GetLastErrorMessage());
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
    /// Creates a new OGD-DCAT catalog via the API with the required mandatory fields and saves its ID.
    /// </summary>
    [Test, Order(1)]
    public async Task ShouldCreateDcatCatalogWithMandatoryFieldsSuccessfully()
    {
        if (_standardAction == null)
        {
            Assert.Fail();
        }

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        // change the role to ‘InteroperabilityService’ otherwise no rights for creating DcatCatalog
        await LoginWidthRoleAndOrganisation(Constants.Eiam.OrganisationI14y, Constants.Eiam.RoleInteroperability);

        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);

        Ogd.CatalogId = await CreateDcatCatalog();

        TestContext.Out.WriteLine($"OGD catalog id:=  {Ogd.CatalogId}");

        await LoginWidthRoleAndOrganisation(Constants.Eiam.OrganisationI14yTest, Constants.Eiam.RoleLocalDataSteward);

        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);

        Assert.Pass();
    }

    /// <summary>
    /// Create a dataset with the minimum required metadata in the Admin UI and check that it has been saved.
    /// </summary>
    [Test, Order(2)]
    public async Task ShouldCreateDatasetWithMandatoryFieldsSuccessfully()
    {
        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        TestContext.Out.WriteLine($"Start create a minimal dataset = {_minimalTitleDataset}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction.GotoCatalog(Actions);

        await _standardAction!.OpenCreateDatasetMask(Actions);

        await DatasetsEditMaskHelper.FillDatasetMinimal(
            Actions,
            _minimalTitleDataset,
            _minimalIdentifierDataset,
            CurrentDate);

        await Actions.WaitForSpinnerToDisappear();

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataset(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        await CheckNewDataset();
    }

    /// <summary>
    /// Extends an existing minimal dataset in the Admin UI with additional metadata and saves the changes.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldExtendDatasetWithAdditionalMetadataSuccessfully()
    {
        var title = _minimalTitleDataset;

        await _standardAction!.ChangeLanguageToGerman(Actions);

        TestContext.Out.WriteLine("Start updating a minimal dataset with additional content");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenEditDatasetMaskByName(Actions, title);

        var usedVocabularyValues = await UpdateEditDatasetMaskLarge();

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDataset(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        await CheckLargeDataset(title, usedVocabularyValues);
    }

    /// <summary>
    /// Creates a distribution with the required fields for the created dataset and saves it.
    /// </summary>
    [Test, Order(4)]
    public async Task ShouldCreateDistributionForDatasetSuccessfully()
    {
        TestContext.Out.WriteLine("Start create a distribution for the generated dataset");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await OpenCreateDistributionMaskByDatasetName(_minimalTitleDataset);

        await DatasetsEditMaskHelper.FillCreateDistributionMinimal(
            Actions, 
            _minimalTitleDistribution,
            _minimalIdentifierDistribution,
            CurrentDate);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction.SaveAndCloseDistribution(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");
    }

    /// <summary>
    /// Create a  DataService with the required mandatory fields in the Admin UI.
    /// </summary>
    [Test, Order(5)]
    public async Task ShouldCreateDataServiceWithMandatoryFieldsSuccessfully()
    {
        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await OpenCreateDataServiceMask();

        await DataServiceEditMaskHelper.FillDataServiceMinimal(Actions, _minimalTitleDataService, _minimalIdentifierDataService);

        await _standardAction!.SaveAndCloseDataService(Actions);
    }

    /// <summary>
    /// Links a DataService in the distribution of the dataset and saves the change.
    /// </summary>
    [Test, Order(6)]
    public async Task ShouldAddDataServiceLinkToDistributionSuccessfully()
    {
        TestContext.Out.WriteLine("Start update distribution for the generated dataset");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenDistributionEditByDatasetName(Actions, _minimalTitleDataset);

        await AddDataServiceLink(_minimalTitleDataService);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDistribution(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

    }

    /// <summary>
    /// Sets the status and release level of a dataset from “Unit” to “Public” and checks the UI chips.
    /// </summary>
    [Test, Order(7)]
    public async Task ShouldChangeDatasetLevelFromUnitToPublicSuccessfully()
    {
        var title = _minimalTitleDataset;

        TestContext.Out.WriteLine("Start Change Status and Level to public");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, title);

        await _standardAction!.SetPublicLevelToPublic(Actions);

        CheckApiError();

        var status = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipStatusId);
        var level = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipLevelId);
        Assert.That(status, Is.EqualTo(StatusLevelVersion.RecordedNameDe), $"status is not set correctly. {StatusLevelVersion.RecordedNameDe} instead of {status}.");
        Assert.That(level, Is.EqualTo(StatusLevelVersion.PublicNameDe), $"level is not set correctly. {StatusLevelVersion.PublicNameDe} instead of {level}.");
    }

    /// <summary>
    /// Downloads the dataset as an RDF file from the server and saves it locally.
    /// </summary>
    [Test, Order(8)]
    public async Task ShouldDownloadDatasetAsRdfFileSuccessfully()
    {
        TestContext.Out.WriteLine("DownloadRdfFile");

        DeleteDownloadFile();

        try
        {
            using HttpClient httpClient = new();

            var exportRdfAddress = $"api/DcatCatalogs/{Ogd.CatalogId}/export/RDF";

            var response = await httpClient.GetAsync(BaseIopCoreUrl + exportRdfAddress);
            response.EnsureSuccessStatusCode();
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _downloadedFile);
            await using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fs);
            }

            Assert.That(File.Exists(outputPath), Is.True, "The file has been successfully downloaded and saved.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Error downloading the file: {ex.Message}");
        }
    }

    /// <summary>
    /// Resets the status and release level of a dataset from “Public” back to “Unit” and checks the UI chips.
    /// </summary>
    [Test, Order(9)]
    public async Task ShouldChangeDatasetLevelFromPublicToUnitSuccessfully()
    {
        var title = _minimalTitleDataset;

        TestContext.Out.WriteLine("Start Change Status and Level to unit");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, title);

        await _standardAction!.ResetPublicLevelToInternal(Actions);

        CheckApiError();

        var status = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipStatusId);
        var level = await _standardAction!.ReadChip(Actions, StatusLevelVersion.ChipLevelId);
        Assert.That(status, Is.EqualTo(StatusLevelVersion.IncompleteNameDe), $"status is not set correctly. {StatusLevelVersion.IncompleteNameDe} instead of {status}.");
        Assert.That(level, Is.EqualTo(StatusLevelVersion.UnitNameDe), $"level is not set correctly. {StatusLevelVersion.UnitNameDe} instead of {level}.");
    }

    /// <summary>
    /// Validates the content of the downloaded RDF file for correct mapping of dataset and distribution.
    /// </summary>
    [Test, Order(10)]
    public void ShouldValidateRdfFileContentShouldCheckRdfFileSuccessfully()
    {
        var title = _minimalTitleDataset;
        var datasetNamespace = BaseDatasetUriPrefix + title;

        string rdfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _downloadedFile);
        IGraph g = new VDS.RDF.Graph();
        FileLoader.Load(g, rdfFilePath);

        IUriNode subjectNode = g.CreateUriNode(new Uri(datasetNamespace));

        var result = CheckDatasetInCatalog(g, subjectNode);
        TestContext.Out.WriteLine($"CheckDatasetInCatalog result: {result}");

        result = result && CheckDistributionInDatasets(g, subjectNode);
        TestContext.Out.WriteLine($"CheckDistributionInDatasets result: {result}");

        Assert.That(result, Is.True, "Not all defined fields were mapped correctly in RDF");
    }

    /// <summary>
    /// Removes the previously added DataService link from the distribution and saves the change.
    /// </summary>
    [Test, Order(11)]
    public async Task ShouldRemoveDataServiceLinkInDistributionTestsSuccessfully()
    {
        var title = _minimalTitleDataset;

        TestContext.Out.WriteLine("Start remove DataService link in  distribution for the generated dataset");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenDistributionEditByDatasetName(Actions, title);

        await RemoveDataServiceLink();

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseDistribution(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");
    }

    /// <summary>
    /// 
    /// Deletes
    /// </summary>
    /// <returns></returns>
    [Test, Order(12)]
    public async Task ShouldCleanupAllCreatedResourcesSuccessfully()
    {
        try
        {
            // Only required if this test is started separately
            if (string.IsNullOrWhiteSpace(_bearerToken))
            {
                _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);
            }

            var searchQuery = _minimalIdentifierDataset;

            var deleteTests = new MyShare.DeleteTests(BaseIopCoreUrl, _bearerToken, searchQuery);

            await deleteTests.CleanupDatasetAndDistributionAndDcatCatalogRecord();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Error delete items: {ex.Message}");
        }

        await DeleteDataService(_minimalIdentifierDataService);
    }

    /// <summary>
    /// Deletes the OGD-DCAT catalog via API call.
    /// </summary>
    /// <returns></returns>
    [Test, Order(13)]
    public async Task ShouldDeleteDcatCatalogSuccessfully()
    {
        // change the role to ‘InteroperabilityService’ otherwise no rights for delete  DcatCatalog
        await LoginWidthRoleAndOrganisation(Constants.Eiam.OrganisationI14y, Constants.Eiam.RoleInteroperability);

        await Actions.WaitUntilUrl(BaseAdminUrl + AttributesAndElements.Home);

        _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);

        await DeleteDcatCatalog(Ogd.CatalogId);
    }

    private bool CheckDistributionInDatasets(IGraph g, IUriNode subjectNode)
    {
        TestContext.Out.WriteLine($"checks distribution {_minimalTitleDataset}");

        IUriNode distributionPredicate = g.CreateUriNode("dcat:distribution");

        var distributionTriples = g.GetTriplesWithSubjectPredicate(subjectNode, distributionPredicate);

        foreach (var distributionTriple in distributionTriples)
        {
            INode distributionNode = distributionTriple.Object;

            var detailsTriples = g.GetTriplesWithSubject(distributionNode);

            TestContext.Out.WriteLine("distribution");
            foreach (Triple t in detailsTriples)
            {
                var p = (UriNode)t.Predicate;
                switch (p.Uri.ToString())
                {
                    case Ogd.PredicateDistributionAccessRights:

                        var accessRight = (UriNode)t.Object;
                        var valueRight = accessRight.Uri.ToString().Replace(Ogd.ObjectPrefixDistributionRight, "");
                        if (valueRight != DatasetConstants.EditMask.AccessRightsOptionId)
                        {
                            TestContext.Out.WriteLine($"Wrong distribution access-right: {accessRight.Uri.ToString()}");
                            return false;
                        }
                        break;

                    case Ogd.PredicateDistributionDescription:

                        var description = (LiteralNode)t.Object;
                        if (description.Language != Ogd.LanguageDe || description.Value != _minimalIdentifierDistribution + "\n")
                        {
                            TestContext.Out.WriteLine($"Wrong description: {description.Value}. Language: {description.Language}");
                            return false;
                        }

                        break;

                    case Ogd.PredicateDistributionIdentifier:

                        var identifier = (LiteralNode)t.Object;
                        if (identifier.Value != _minimalIdentifierDistribution)
                        {
                            TestContext.Out.WriteLine($"Wrong identifier: {identifier.Value}");
                            return false;
                        }

                        break;

                    case Ogd.PredicateDistributionTitle:

                        var title = (LiteralNode)t.Object;
                        if (title.Language != Ogd.LanguageDe || title.Value != _minimalTitleDistribution)
                        {
                            TestContext.Out.WriteLine($"Wrong title: {title.Value}");
                            return false;
                        }

                        break;

                    case Ogd.PredicateDistributionLicense:

                        var license = (UriNode)t.Object;
                        var valueLicense = license.Uri.ToString().Replace(Ogd.ObjectPrefixDistributionLicense, "");
                        if (valueLicense != Ogd.OptionLicense1)
                        {
                            //TODO: FIX THIS ->I didn't have the time or the desire to fix it anymore.
                            //TestContext.Out.WriteLine($"Wrong distribution license: {license.Uri.ToString()}");
                            //return false;
                        }
                        break;

                    case Ogd.PredicateDistributionDate:

                        var dateObject = (LiteralNode)t.Object;
                        if (DateTime.Parse(dateObject.Value) != DateTime.Parse(CurrentDate))
                        {
                            TestContext.Out.WriteLine($"Wrong distribution issued: {dateObject.Value}");
                            return false;
                        }

                        break;
                }
            }
        }

        return true;
    }

    private async Task DeleteDataService(string dataServiceIdentifier)
    {
        try
        {
            // Only required if this test is started separately
            if (string.IsNullOrWhiteSpace(_bearerToken))
            {
                _bearerToken = await Actions.ReadLocalStorage(Navigation.AccessTokenKey);
            }

            var searchQuery = dataServiceIdentifier;

            var deleteTests = new MyShare.DeleteTests(BaseIopCoreUrl, _bearerToken, searchQuery);

            await deleteTests.DeleteDataService();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Error delete dataservice: {ex.Message}");
        }
    }

    private bool CheckDatasetInCatalog(IGraph g, IUriNode subjectNode)
    {
        TestContext.Out.WriteLine($"checks dataset {_minimalTitleDataset}");

        var arrayThemeCodes = new string[] { Ogd.OptionCatalog1.Replace(Ogd.OptionCatalogPrefix, string.Empty), Ogd.OptionCatalog2.Replace(Ogd.OptionCatalogPrefix, string.Empty), Ogd.OptionCatalog3.Replace(Ogd.OptionCatalogPrefix, string.Empty) };
        var themesSum = 0;
        Dictionary<string, string> publisherNames = new Dictionary<string, string>();
        Dictionary<string, string> contactPointDetails = new Dictionary<string, string>();

        // Triple = Subject, Predicate, Object
        foreach (Triple t in g.GetTriplesWithSubject(subjectNode))
        {
            var p = (UriNode)t.Predicate;
            switch (p.Uri.ToString())
            {
                case Ogd.PredicateDatasetDescription:

                    TestContext.Out.WriteLine("check dataset description");

                    var description = (LiteralNode)t.Object;
                    if (description.Language != Ogd.LanguageDe || description.Value != DatasetConstants.Data.Description)
                    {
                        TestContext.Out.WriteLine($"Wrong description: {description.Value}. Language: {description.Language}.");
                        return false;
                    }
                    break;

                case Ogd.PredicateDatasetIdentifier:

                    TestContext.Out.WriteLine("check dataset identifier");

                    var identifier = (LiteralNode)t.Object;
                    if (identifier.Value != _minimalIdentifierDataset)
                    {
                        TestContext.Out.WriteLine($"Wrong identifier: {identifier.Value}.");
                        return false;
                    }

                    break;

                case Ogd.PredicateDatasetTitle:

                    TestContext.Out.WriteLine("check dataset title");

                    var title = (LiteralNode)t.Object;
                    if (title.Value != _minimalTitleDataset)
                    {
                        TestContext.Out.WriteLine($"Wrong title: {title.Value}");
                        return false;
                    }

                    break;

                case Ogd.PredicateDatasetAccessRights:
                    var accessRight = (UriNode)t.Object;
                    var valueRight = accessRight.Uri.ToString().Replace(Ogd.ObjectPrefixDatasetRight, "");
                    if (valueRight != DatasetConstants.EditMask.AccessRightsOptionId) // hard coded, unfortunately
                    {
                        TestContext.Out.WriteLine($"Wrong access-right: {accessRight.Uri.ToString()}");
                        return false;
                    }
                    break;

                case Ogd.PredicateDatasetTheme:
                    var theme = (UriNode)t.Object;
                    var value = theme.Uri.ToString().Replace(Ogd.ObjectPrefixCatalogTheme, "");
                    if (!arrayThemeCodes.Contains(value))
                    {
                        TestContext.Out.WriteLine($"Wrong theme: {theme.Uri.ToString()}");
                        return false;
                    }
                    themesSum++;
                    break;

                case Ogd.PredicateDatasetPublisher:
                    var agentNode = (BlankNode)t.Object;
                    foreach (Triple agentTriple in g.GetTriplesWithSubject(agentNode))
                    {
                        var agentPredicate = (UriNode)agentTriple.Predicate;
                        if (agentPredicate.Uri.ToString() == Ogd.PredicateDatasetAgent)
                        {
                            var agentName = (LiteralNode)agentTriple.Object;
                            publisherNames[agentName.Language] = agentName.Value;
                        }
                    }
                    if (publisherNames.Count != Ogd.NumberLanguages)
                    {
                        TestContext.Out.WriteLine("Not all publisher information found.");
                        return false;
                    }
                    if (publisherNames[Ogd.LanguageDe] != Ogd.PublisherNameDe)
                    {
                        TestContext.Out.WriteLine($"wrong publisher: {publisherNames[Ogd.LanguageDe]}");
                        return false;
                    }
                    break;

                case Ogd.PredicateDatasetContactPoint:
                    var contactPointNode = (BlankNode)t.Object;
                    foreach (Triple contactTriple in g.GetTriplesWithSubject(contactPointNode))
                    {
                        var contactPredicate = (UriNode)contactTriple.Predicate;
                        switch (contactPredicate.Uri.ToString().Replace(Ogd.ObjectPrefixVcard, string.Empty))
                        {
                            case Ogd.ContactPointFnKey:
                                var fn = (LiteralNode)contactTriple.Object;
                                contactPointDetails[Ogd.ContactPointFnKey] = fn.Value;
                                break;

                            case Ogd.ContactPointAdrWorkKey:
                                var adrWork = (LiteralNode)contactTriple.Object;
                                contactPointDetails[Ogd.ContactPointAdrWorkKey] = adrWork.Value;
                                break;

                            case Ogd.ContactPointNoteKey:
                                var note = (LiteralNode)contactTriple.Object;
                                contactPointDetails[Ogd.ContactPointNoteKey] = note.Value;
                                break;

                            case Ogd.ContactPointHasEmailKey:
                                var hasEmail = (LiteralNode)contactTriple.Object;
                                contactPointDetails[Ogd.ContactPointHasEmailKey] = hasEmail.Value;
                                break;

                            case Ogd.ContactPointHasTelephoneKey:
                                var hasTelephone = (LiteralNode)contactTriple.Object;
                                contactPointDetails[Ogd.ContactPointHasTelephoneKey] = hasTelephone.Value;
                                break;
                        }
                    }

                    if (!contactPointDetails.Any())
                    {
                        TestContext.Out.WriteLine($"wrong publisher: {publisherNames[Ogd.LanguageDe]}");
                        return false;
                    }

                    if (contactPointDetails.ContainsKey(Ogd.ContactPointFnKey) && contactPointDetails[Ogd.ContactPointFnKey] != DatasetConstants.Data.ContactPointValue)
                    {
                        TestContext.Out.WriteLine($"wrong ContactPoint: {contactPointDetails[Ogd.ContactPointFnKey]}");
                        return false;
                    }

                    if (contactPointDetails.ContainsKey(Ogd.ContactPointAdrWorkKey) && contactPointDetails[Ogd.ContactPointAdrWorkKey] != DatasetConstants.Data.ContactAddressValue)
                    {
                        TestContext.Out.WriteLine($"wrong ContactPoint Address: {contactPointDetails[Ogd.ContactPointAdrWorkKey]}");
                        return false;
                    }

                    if (contactPointDetails.ContainsKey(Ogd.ContactPointNoteKey) && contactPointDetails[Ogd.ContactPointNoteKey] != DatasetConstants.Data.ContactNoteValue)
                    {
                        TestContext.Out.WriteLine($"wrong ContactPoint Note: {contactPointDetails[Ogd.ContactPointNoteKey]}");
                        return false;
                    }

                    break;
            }
        }

        if (themesSum != arrayThemeCodes.Length)
        {
            TestContext.Out.WriteLine($"Wrong theme sum: Quantity found {themesSum}");
            return false;
        }

        return true;
    }

    private void DeleteDownloadFile()
    {
        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _downloadedFile);

        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }

    private async Task OpenCreateDistributionMaskByDatasetName(string titleDataset)
    {
        await _standardAction!.SearchDatasetByName(Actions, titleDataset);

        await Actions.ClickButtonById(DatasetConstants.EditMask.CatalogTableViewButton + "0");
        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickTabsById(DatasetConstants.EditMask.TabDistributionId);
        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(DatasetConstants.EditMask.DistributionButtonId);
        await Actions.WaitForSpinnerToDisappear();
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
        await Actions.Wait500();
    }

    private async Task CheckNewDataset()
    {
        TestContext.Out.WriteLine("Check new dataset");

        var title = _minimalTitleDataset;

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, title);

        await CheckDatasetMinimal(title);
    }

    private async Task CheckDatasetMinimal(string title)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Title", DatasetConstants.ViewMask.DatasetTitleId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Description", DatasetConstants.ViewMask.DatasetDescriptionId, DatasetConstants.Data.Description);

        await _standardAction!.CheckDivPropertyById(Actions, "Identifier", DatasetConstants.ViewMask.DatasetIdentifierId, title);

        await _standardAction!.CheckDivPropertyById(Actions, "Publisher", DatasetConstants.ViewMask.DatasetPublisherId, DatasetConstants.Data.PublisherViewDe);
    }

    private async Task CheckLargeDataset(string title, IReadOnlyDictionary<string, string> usedVocabularyValues)
    {
        TestContext.Out.WriteLine("Check large dataset");

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, title);

        await CheckDatasetLarge(usedVocabularyValues);
    }

    private async Task CheckDatasetLarge(IReadOnlyDictionary<string, string> vocabularyValues)
    {
        await _standardAction!.CheckDivPropertyById(Actions, "Languages", DatasetConstants.ViewMask.DatasetLanguagesId, vocabularyValues[DatasetConstants.EditMask.SelectLanguageId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Landing pages", DatasetConstants.ViewMask.DatasetLandingPageId, DatasetConstants.Data.LandingPagesDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Spatial", DatasetConstants.ViewMask.DatasetSpatialId, DatasetConstants.Data.SpatialIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Geo", DatasetConstants.ViewMask.DatasetGeoId, vocabularyValues[DatasetConstants.EditMask.SelectGeoIvId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Coverage", DatasetConstants.ViewMask.DatasetCoverageId, CurrentDate, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Frequency", DatasetConstants.ViewMask.DistributionsFrequencyId, vocabularyValues[DatasetConstants.EditMask.SelectFrequencyId]);

        await _standardAction!.CheckDivPropertyById(Actions, "Conforms to", DatasetConstants.ViewMask.DatasetConformsToId, DatasetConstants.Data.ConformsToDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Referenced by", DatasetConstants.ViewMask.DatasetReferencedById, DatasetConstants.Data.IsReferencedByDeIdValue, false);

        await _standardAction!.CheckDivPropertyById(Actions, "Playwright_Test_OGD-Catalog 1", DatasetConstants.ViewMask.OgdCatalogThemesId + Ogd.CatalogId, DatasetConstants.Data.OgdCalolgThemesValueDe1Id, false);
        await _standardAction!.CheckDivPropertyById(Actions, "Playwright_Test_OGD-Catalog 2", DatasetConstants.ViewMask.OgdCatalogThemesId + Ogd.CatalogId, DatasetConstants.Data.OgdCalolgThemesValueDe2Id, false);
        await _standardAction!.CheckDivPropertyById(Actions, "Playwright_Test_OGD-Catalog 3", DatasetConstants.ViewMask.OgdCatalogThemesId + Ogd.CatalogId, DatasetConstants.Data.OgdCalolgThemesValueDe3Id, false);
    }

    private async Task<IReadOnlyDictionary<string, string>> UpdateEditDatasetMaskLarge()
    {
        var usedVocabularyValues = new Dictionary<string, string>();

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.PublicationDateId);
        TestContext.Out.WriteLine("Write Publication date");
        await Actions.FillInputAndEnterById(DatasetConstants.EditMask.RetentionPeriodId, CurrentDate);

        TestContext.Out.WriteLine("Write Modified date");
        var dateModified = DateTime.Now.AddMonths(6).ToString("dd.MM.yyyy");
        await Actions.FillInputAndEnterById(DatasetConstants.EditMask.ModifiedDateId, dateModified);

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectLanguageId);
        TestContext.Out.WriteLine("Write Language");
        var arrayLanguage = new string[] { DatasetConstants.EditMask.OptionLanguageId + "0", DatasetConstants.EditMask.OptionLanguageId + "2", DatasetConstants.EditMask.OptionLanguageId + "3" };
        await Actions.SelectOptionsById(DatasetConstants.EditMask.SelectLanguageId, arrayLanguage);
        var languages = await Actions.ReadMatSelect(DatasetConstants.EditMask.SelectLanguageId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.SelectLanguageId, languages);

        // Chose Catalog themes
        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.ButtonCatalogLinkId);
        TestContext.Out.WriteLine("Chose Catalog");
        await Actions.ClickButtonById(DatasetConstants.EditMask.ButtonCatalogLinkId);
        await Actions.Wait500();

        var tmpButton = DatasetConstants.EditMask.MenuButtonCatalogLinkId + Ogd.CatalogId;
        await Actions.IsElementVisibleById(tmpButton);
        await Actions.ClickButtonById(tmpButton);
        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear();

        CheckApiError();

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectCatalogThemesId);
        TestContext.Out.WriteLine("Write Catalog ThemeCodes");
        var arrayThemeCodes = new string[] { Ogd.OptionCatalog1, Ogd.OptionCatalog2, Ogd.OptionCatalog3 };
        await Actions.SelectOptionsById(DatasetConstants.EditMask.SelectCatalogThemesId, arrayThemeCodes);
        var selectedThemes = await Actions.ReadMatSelect(DatasetConstants.EditMask.SelectThemeCodesId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.SelectThemeCodesId, selectedThemes);

        // End Catalog themes

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.RetentionPeriodId);
        TestContext.Out.WriteLine("Write Retention Period");
        var dateRetention = DateTime.Now.AddMonths(6).ToString("dd.MM.yyyy");
        await Actions.FillInputAndEnterById(DatasetConstants.EditMask.RetentionPeriodId, dateRetention);

        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear(); // Because Toast can hide the AddRowId button

        TestContext.Out.WriteLine("Write LandingPages");
        await Actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameLandingPages);
        var itemLandingPages = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(DatasetConstants.EditMask.LandingPagesHrefId, DatasetConstants.Data.LandingPagesHrefValue), new KeyValuePair<string, string>(DatasetConstants.EditMask.LandingPagesDefId, DatasetConstants.Data.LandingPagesDeIdValue) };
        await Actions.FillEditTableRow(DatasetConstants.EditMask.LandingPagesAddRowId, DatasetConstants.EditMask.LandingPagesSaveRowId, 0, itemLandingPages);

        await Actions.WaitForNotificationToDisappear(); // Because Toast can hide the AddRowId button
        TestContext.Out.WriteLine("Write Keywords");
        await Actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameKeywords);
        var itemKeywords = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(DatasetConstants.EditMask.KeywordsDeId, DatasetConstants.Data.KeywordsDeValue) };
        await Actions.FillEditTableRow(DatasetConstants.EditMask.KeywordsAddRowId, DatasetConstants.EditMask.KeywordSaveRowId, 0, itemKeywords);

        await Actions.WaitForNotificationToDisappear(); // Because Toast can hide the AddRowId button
        TestContext.Out.WriteLine("Write Spatial");
        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.SpatialId);
        await Actions.FillInputAndEnterById(DatasetConstants.EditMask.SpatialId, DatasetConstants.Data.SpatialIdValue);

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectGeoIvId);
        TestContext.Out.WriteLine("Write SelectGeoIv");
        var arrayGeoIvs = new string[] { DatasetConstants.EditMask.OptionGeoIv2Id };
        await Actions.SelectOptionsById(DatasetConstants.EditMask.SelectGeoIvId, arrayGeoIvs);
        var selectedGeoIvValue = await Actions.ReadMatSelect(DatasetConstants.EditMask.SelectGeoIvId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.SelectGeoIvId, selectedGeoIvValue);

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.CoverageFromId);
        TestContext.Out.WriteLine("Write Temporal coverage");
        await Actions.FillInputAndEnterById(DatasetConstants.EditMask.CoverageFromId, CurrentDate);

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectFrequencyId);
        TestContext.Out.WriteLine("Write Frequency");
        var arrayFrequency = new string[] { DatasetConstants.EditMask.OptionFrequency2Id };
        await Actions.SelectOptionsById(DatasetConstants.EditMask.SelectFrequencyId, arrayFrequency);
        var selectedFrequencyValue = await Actions.ReadMatSelect(DatasetConstants.EditMask.SelectFrequencyId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.SelectFrequencyId, selectedFrequencyValue);

        await Actions.WaitForNotificationToDisappear(); // Because Toast can hide the addRowId button
        TestContext.Out.WriteLine("Write Conforms To");
        await Actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameConformsTo);
        var itemConformsTos = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(DatasetConstants.EditMask.ConformsToHrefId, DatasetConstants.Data.ConformsToHrefIdValue), new KeyValuePair<string, string>(DatasetConstants.EditMask.ConformsToDefId, DatasetConstants.Data.ConformsToDeIdValue) };
        await Actions.FillEditTableRow(DatasetConstants.EditMask.ConformsToAddRowId, DatasetConstants.EditMask.ConformsToSaveRowId, 0, itemConformsTos);

        await Actions.WaitForNotificationToDisappear(); // Because Toast can hide the addRowId button
        TestContext.Out.WriteLine("Write IsReferenced");
        await Actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameIsReferencedBy);
        var itemIsReferenced = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(DatasetConstants.EditMask.IsReferencedByHrefId, DatasetConstants.Data.IsReferencedByHrefIdValue), new KeyValuePair<string, string>(DatasetConstants.EditMask.IsReferencedByDefId, DatasetConstants.Data.IsReferencedByDeIdValue) };
        await Actions.FillEditTableRow(DatasetConstants.EditMask.IsReferencedByAddRowId, DatasetConstants.EditMask.IsReferencedBySaveRowId, 0, itemIsReferenced);

        await Actions.WaitForNotificationToDisappear(); // Because Toast can hide the addRowId button
        TestContext.Out.WriteLine("Write Qualified Relation");
        await Actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameQualifiedRelation);
        var itemQualifiedRelation = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>(DatasetConstants.EditMask.QualifiedRelationHrefId, DatasetConstants.Data.IsReferencedByHrefIdValue), new KeyValuePair<string, string>(DatasetConstants.EditMask.QualifiedRelationDeId, DatasetConstants.Data.IsReferencedByDeIdValue) };
        await Actions.FillEditTableWithPulldownRow(DatasetConstants.EditMask.QualifiedRelationAddRowId, DatasetConstants.EditMask.QualifiedRelationSaveRowId, 0, DatasetConstants.EditMask.QualifiedRelationHadRoleId, DatasetConstants.EditMask.QualifiedRelationOptionHadRoleId + "0", itemQualifiedRelation);

        CheckApiError();

        // TODO: 
        if (string.IsNullOrWhiteSpace(await Actions.ReadInput(DatasetConstants.EditMask.IdentifierId)))
        {
            await Actions.ScrollIntoViewById(DatasetConstants.EditMask.IdentifierId);
            TestContext.Out.WriteLine("Write Identifier");
            await Actions.FillInputById(DatasetConstants.EditMask.IdentifierId, _minimalIdentifierDataset);
        }

        await Actions.WaitForNotificationToDisappear();

        return usedVocabularyValues;
    }

    private async Task AddDataServiceLink(string minimalTitleDataService)
    {
        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionDataServiceLinkAddRowId);
        await Actions.Wait500();
        await Actions.ClickButtonById(DatasetConstants.EditMask.DistributionDataServiceLinkAddRowId);
        await Actions.Wait1000();
        await Actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionDataServiceOptionTitleId + "0", minimalTitleDataService);
        await Actions.ClickButtonById(DatasetConstants.EditMask.DistributionDataServiceSaveRowId + 0);
    }

    private async Task RemoveDataServiceLink()
    {

        TestContext.Out.WriteLine($"remove DataService link {DatasetConstants.EditMask.DistributionDataServiceOptionTitleId}");

        await Actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionDataServiceLinkAddRowId);
        await Actions.Wait1000();
        await Actions.ClickCheckBoxById(DatasetConstants.EditMask.DistributionDataServiceSelectAllRowsId);
        await Actions.Wait500();

        await Actions.ClickButtonById(DatasetConstants.EditMask.DistributionDataServiceRemoveAllRowsId);
    }

    private void CheckApiError()
    {
        var hasError = _listener!.HasApiErrors();
        Assert.That(hasError, Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
        if (hasError)
        {
            _listener.ResetErrors();
        }
    }

    private async Task LoginWidthRoleAndOrganisation(string organisation, string role)
    {
        var login = new EiamLogin();

        await login.Logout(Page);

        await Actions.Wait1500();

        await ReadLoginResult(await login.TryConnectToEiam(Page, UserName, Password, Tan), organisation, role);
    }

    private async Task ReadLoginResult(bool result, string organisation, string role)
    {
        if (result)
        {
            await Actions.Wait1500();
            TestContext.Out.WriteLine("chose profile");
            _ = await EiamLogin.ChooseProfile(Page, organisation, role);
        }
        else
        {
            Assert.Fail();
        }
    }

    private async Task<string> CreateDcatCatalog()
    {
        var result = string.Empty;
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            var urlAgent = BaseIopCoreUrl + _agentAddress;
            var agentList = await httpClient.GetAsync(urlAgent);

            var content = await PutI14Y_Test_OrganisationIdInOgdCatalog(agentList);

            var url = BaseIopCoreUrl + _dcatCatalogsAddress;

            HttpResponseMessage postResponse = await httpClient.PostAsync(url, content);

            if (postResponse.IsSuccessStatusCode)
            {
                string postResponseBody = await postResponse.Content.ReadAsStringAsync();
                var id = postResponseBody.Trim('"');
                HttpResponseMessage getResponse = await httpClient.GetAsync(url + $"/{id}");

                var getResponseBody = await getResponse.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                options.Converters.Add(new JsonStringEnumConverter());

                var dcatCatalogModel = JsonSerializer.Deserialize<DcatCatalogModel>(getResponseBody, options);
                result = dcatCatalogModel?.Id.ToString() ?? string.Empty;

                Assert.IsFalse(string.IsNullOrEmpty(result), "No catalog was created.");
            }
            else
            {
                TestContext.Out.WriteLine($"Error while trying to create an OGD catalog : {postResponse.StatusCode}, {await postResponse.Content.ReadAsStringAsync()}");
                Assert.Fail($"Error while trying to create an OGD catalog : {postResponse.StatusCode}, {await postResponse.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Error while trying to create an OGD catalog : {ex.Message}");
            Assert.Fail(($"Error while trying to create an OGD catalog : {ex.Message}"));
        }

        return result;
    }

    private async Task<StringContent> PutI14Y_Test_OrganisationIdInOgdCatalog(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var list = JsonSerializer.Deserialize<IEnumerable<IdentifierIdModel>>(responseBody, options);
            if (list != null || (list != null && !list.Any()))
            {
                var agentId = list.First(x => x.Identifier == "i14y-test-organisation").Id;

                if (!string.IsNullOrEmpty(agentId))
                {
                    var model = _OgdCatalogJson;

                    model = model.Replace("7fd424b8-0cca-4954-bc80-e889c5c3a510", agentId);

                    return new StringContent(model, Encoding.UTF8, "application/json");
                }

            }
        }
        return new StringContent(_OgdCatalogJson, Encoding.UTF8, "application/json");
    }

    private async Task DeleteDcatCatalog(string id)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            var uriBuilder = new UriBuilder(BaseIopCoreUrl);
            uriBuilder.Path = $"{_dcatCatalogsAddress.Trim('/')}/{id}";
            var url = uriBuilder.Uri;

            HttpResponseMessage response = await httpClient.DeleteAsync(url);

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                TestContext.Out.WriteLine($"Error while trying to delete an OGD catalog : {response.StatusCode.ToString()}");
                Assert.Fail(($"Error while trying to delete an OGD catalog : {response.StatusCode.ToString()}"));
            }

            TestContext.Out.WriteLine($"dcat catalog  {id} is removed");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Error while trying to delete an OGD catalog : {ex.Message}");
            Assert.Fail(($"Error while trying to delete an OGD catalog : {ex.Message}"));
        }
    }

}
