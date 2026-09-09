using Bfs.Iop.Public.Testautomation.Constants;
using Bfs.Iop.Public.Testautomation.Helpers;
using Bfs.Iop.Public.Testautomation.Share;
using Bfs.Iop.Test.Abstraction.Shared;

namespace Bfs.Iop.Public.Testautomation.DatasetTests;

[Order(2)]
[TestFixture ]
public class ReadDatasetTests : PlaywrightSetup
{
    private StandardTask? _standardAction;
    private Listener? _listener;

    [SetUp]
    public void SetupInternal()
    {
        _standardAction = new StandardTask();
        _listener = new Listener(Page);
        _listener.RecognizeApiErrors();
        GotoDatasetMaskPage().Wait();

        TestContext.WriteLine($"Current Page Url: {Page.Url}");
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

    [Test, Order(1)]
    public async Task ShouldSearchDatasetSuccessfully()
    {
        var res = await Actions.ValidateDivContent(Datasets.DatasetTitleId, DatasetData.DatasetTitle);
        Assert.That(  res, Is.True, $"Title not congruent with expected value");

        res = await Actions.ValidateChipContent(Datasets.DatasetRegistrationstatusId, DatasetData.DatasetRegistrationstatusDe);
        Assert.That(res, Is.True, $"Registrationstatus not congruent with expected value");
    }

    [Test, Order(2)]
    public async Task ShouldReadDatasetSuccessfully()
    {
        TestContext.Out.WriteLine($"Check the contents of the dataset {DatasetData.DatasetTitle}");

        await Actions.ScrollIntoViewById(Datasets.DescriptionDescriptionId);
        var res = await Actions.ValidateDivContent(Datasets.DescriptionDescriptionId, DatasetData.DatasetDescriptionDe,true);
        Assert.That(res, Is.True, "Description not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionIdentifierId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionIdentifierId, DatasetData.DatasetIdentifierDe);
        Assert.That(res, Is.True, "Identifier not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionIssuedId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionIssuedId, DatasetData.DatasetIssuedDe);
        Assert.That(res, Is.True, "Issue Date not congruent with expected value");

        //res = await Actions.ValidateDivContent(Datasets.DatasetModifiedContentId, DatasetData.DatasetModifiedDe);
        //Assert.That(res, Is.True, "Modified Date not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionPublisherId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionPublisherId, DatasetData.DatasetPublisherDe);
        Assert.That(res, Is.True, "Publisher not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionContactPointsId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionContactPointsId, DatasetData.DatasetContactPointsDe, false);
        Assert.That(res, Is.True, "Contact Points not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionContactPointsId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionContactPointsId, DatasetData.DatasetContactPointsDe2, false);
        Assert.That(res, Is.True, "Contact Points not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionLanguagesId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionLanguagesId, DatasetData.DatasetLanguagesDe);
        Assert.That(res, Is.True, "Languages not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionKeywordsId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionKeywordsId, DatasetData.DatasetKeywordsDe, false);
        Assert.That(res, Is.True, "Keywords not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionLandingPageId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionLandingPageId, DatasetData.DatasetLandingPageDe, false);
        Assert.That(res, Is.True, "Landing page not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionAccessRightsId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionAccessRightsId, DatasetData.DatasetAccessRightDe);
        Assert.That(res, Is.True, "Access rights not congruent with expected value");

        await Actions.ScrollIntoViewById(Datasets.DescriptionSpatialId);
        res = await Actions.ValidateDivContent(Datasets.DescriptionSpatialId, DatasetData.DatasetSpatialDe);
        Assert.That(res, Is.True, "Spatial not congruent with expected value");

        //await Actions.ScrollIntoViewById(Datasets.DatasetFrequencyContentId);
        //res = await Actions.ValidateDivContent(Datasets.DatasetFrequencyContentId, DatasetData.DatasetFrequencyDe);
        //Assert.That(res, Is.True, "Frequency not congruent with expected value");

        //res = await Actions.ValidateDivContent(Datasets.DatasetVersionContentId, DatasetData.DatasetVersionDe);
        //Assert.That(res, Is.True, "Version not congruent with expected value");

        //res = await Actions.ValidateDivContent(Datasets.DatasetVersionContentId, DatasetData.DatasetVersionDe);
        //Assert.That(res, Is.True, "Version not congruent with expected value");

        //res = await Actions.ValidateDivContent(Datasets.DatasetVersionContentId, DatasetData.DatasetVersionDe);
        //Assert.That(res, Is.True, "Version not congruent with expected value");

        //res = await Actions.ValidateDivContent(Datasets.DatasetVersionNotesContentId, DatasetData.DatasetVersionNotesDe);
        //Assert.That(res, Is.True, "Version Note not congruent with expected value");

        //res = await Actions.ValidateDivContent(Datasets.DatasetPreviousVersionContentId, DatasetData.DatasetPreviousVersionDe);
        //Assert.That(res, Is.True, "Previous Version not congruent with expected value");
    }

    private async Task GotoDatasetMaskPage()
    {
        TestContext.Out.WriteLine($"Open the dataset {DatasetData.DatasetTitle}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.Datasets);

        await _standardAction!.ShowResultInTable(Actions);

        await _standardAction!.OpenViewDatasetMaskByName(Actions, DatasetData.DatasetTitle);

        await Actions.WaitForSpinnerToDisappear();

    }
}