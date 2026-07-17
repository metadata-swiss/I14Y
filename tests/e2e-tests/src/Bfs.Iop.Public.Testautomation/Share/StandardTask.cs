using Bfs.Iop.Public.Testautomation.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using static Bfs.Iop.Public.Testautomation.Constants.Search;

namespace Bfs.Iop.Public.Testautomation.Share;

public class StandardTask
{
    #region Language

    private async Task ChangeLanguages(Wrapper action, string id)
    {
        var LanguageDropdown = await action.FindElementById(Navigation.LanguageDropdownId);
        if (LanguageDropdown != null)
        {
            await LanguageDropdown.ClickAsync();
            await action.Wait500();
            await ChoseLanguage(action, id);

        }
        else
        {
            Assert.Fail("language dropdown not found!");
        }
    }

    private async Task ChoseLanguage(Wrapper action, string id)
    {
        var LanguageDropdown = await action.FindElementById(id);
        if (LanguageDropdown != null)
        {
            await LanguageDropdown.ClickAsync();
        }
        else
        {
            Assert.Fail("language dropdown not found!");
        }
    }

    public async Task ChangeLanguageToGerman(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to German language");
        await ChangeLanguages(action, Navigation.LanguageOptionDeId);
    }

    public async Task ChangeLanguageToFrench(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to French language");
        await ChangeLanguages(action, Navigation.LanguageOptionFrId);
    }

    public async Task ChangeLanguageTotalian(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to talian language");
        await ChangeLanguages(action, Navigation.LanguageOptionItId);
    }


    public async Task ChangeLanguageToEnglish(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to a stiff English accent");
        await ChangeLanguages(action, Navigation.LanguageOptionFrId);
    }


    #endregion  Language


    #region Catalog

    public async Task GotoCatalog(Wrapper action)
    {
        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        var catalogTab = await action.FindElementById(Navigation.MainCatalogId);
        if (catalogTab != null)
        {
            await catalogTab.ClickAsync();
        }
        else
        {
            Assert.Fail("catalog tab not found!");
        }

        await action.Wait1000();
    }

    public async Task ShowResultInTable(Wrapper action)
    {
        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        var tableButton = await action.FindElementById(Search.FilterShowTableButtonId);
        if (tableButton != null)
        {
            await tableButton.ClickAsync();
        }
        else
        {
            Assert.Fail("filter visibility show table not found!");
        }

        await action.Wait500();
    }

    public async Task<bool> IsResultTableVisible(Wrapper action)
    {
        await action.WaitForSpinnerToDisappear();
        var table = await action.FindElementById(Search.CatalogTableId);
        await action.WaitForSpinnerToDisappear();

        if (table != null)
        {
            return true;
        }

        return false;
    }

    public async Task SelectFilterTab(Wrapper action, SearchType searchTab)
    {
        await action.WaitForSpinnerToDisappear();

        switch (searchTab)
        {
            case SearchType.All:
                await action.ClickTabsById(Search.CatalogTabAllId);
                break;
            case SearchType.Datasets:
                await action.ClickTabsById(Search.CatalogTabDatasetsId);
                break;
            case SearchType.PublicServices:
                await action.ClickTabsById(Search.CatalogTabPublicServicesId);
                break;
            case SearchType.Api:
                await action.ClickTabsById(Search.CatalogTabDataServicesId);
                break;
            case SearchType.Concepts:
                await action.ClickTabsById(Search.CatalogTabConceptsId);
                break;
            case SearchType.Opendata:
                await action.ClickTabsById(Search.CatalogTabOpendataId);
                break;
            case SearchType.Geocat:
                await action.ClickTabsById(Search.CatalogTabGeocatId);
                break;
            default:
                throw new ArgumentException($"Unexpected search tab type: {searchTab}");
        }

        await action.Wait500();
    }



    #endregion Catalog

    #region Dataset

    public async Task OpenViewDatasetMaskByName(Wrapper action, string titleDataset)
    {
        TestContext.Out.WriteLine($"Open view mask for {titleDataset}");

        await SearchDatasetByName(action, titleDataset);

        await action.WaitForSpinnerToDisappear();


        await action.ClickButtonById(Search.CatalogTableViewButton + "0");
        await action.WaitForSpinnerToDisappear();

        
        await action.Wait1000();
    }

    public async Task SearchDatasetByName(Wrapper action, string titleDataset)
    {
        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        TestContext.Out.WriteLine($"Search {titleDataset}");
        await action.SearchById(Search.CatalogSearchId, titleDataset);

        await action.WaitForSpinnerToDisappear();

        var table = action.FindElementById(Search.CatalogTableId);

        if (table != null)
        {
            var sum = await action.SumRowFromTable(Search.CatalogTableId);

            Assert.That(sum >= 1, Is.True, $"A dataset was expected in the search '{titleDataset}', but none was found");
        }
        else
        {
            Assert.Fail("catalog-table not found");
        }

    }

    #endregion Dataset

    
}
