using Bfs.Iop.Public.Testautomation.Constants;
using Bfs.Iop.Public.Testautomation.Helpers;
using Bfs.Iop.Public.Testautomation.Share;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Shared;


namespace Bfs.Iop.Public.Testautomation.Searches;

[Order(1)]
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class SearchTests : PlaywrightSetup
{
    private StandardTask? _standardAction;
    private Listener? _listener;

    [SetUp]
    public void SetupInternal()
    {
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

    [Test, Order(1)]
    public async Task ShouldGoToSearchMaskSuccessfully()
    {

        await _standardAction!.ChangeLanguageToGerman(Actions);

        Assert.That(Page.Url.Contains(Navigation.LanguageAddressDeId), Is.True);
        Assert.That(Page.Url.Contains(BasePublicUrl), Is.True);
        Assert.That(Page.Url.Contains(AttributesAndElements.Home), Is.True);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.ShowResultInTable(Actions);

       var isResultTableVisible =  await _standardAction!.IsResultTableVisible(Actions);

        Assert.That(isResultTableVisible, Is.True);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.All);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.Datasets);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.PublicServices);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.Api);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.Concepts);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.Opendata);

        await _standardAction!.SelectFilterTab(Actions, Search.SearchType.Geocat);

    }

    

}
