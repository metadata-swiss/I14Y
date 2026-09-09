using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Ds = Bfs.Iop.Admin.Testautomation.Constants.DatasetConstants;
using Nav = Bfs.Iop.Admin.Testautomation.Constants.Navigation;

namespace Bfs.Iop.Admin.Testautomation.Shared;

internal sealed class StandardTask
{
    #region Language

    private async Task ChangeLanguages(Wrapper action, string id)
    {
        var languageDropdown = await action.FindElementById(Nav.LanguageDropdownId);
        if (languageDropdown != null)
        {
            await languageDropdown.ClickAsync();
            await action.Wait500();
            await ChooseLanguage(action, id);
        }
        else
        {
            Assert.Fail("language dropdown not found!");
        }
    }

    private async Task ChooseLanguage(Wrapper action, string id)
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
        await ChangeLanguages(action, Nav.LanguageOptionDeId);
    }

    public async Task ChangeLanguageToFrench(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to French language");
        await ChangeLanguages(action, Nav.LanguageOptionFrId);
    }

    public async Task ChangeLanguageToItalian(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to Italian language");
        await ChangeLanguages(action, Nav.LanguageOptionItId);
    }

    public async Task ChangeLanguageToEnglish(Wrapper action)
    {
        TestContext.Out.WriteLine("Switch to a stiff English accent");
        await ChangeLanguages(action, Nav.LanguageOptionFrId);
    }

    #endregion 

    #region Catalog

    public async Task GotoCatalog(Wrapper action)
    {
        await action.WaitForSpinnerToDisappear();

        var catalogTab = await action.FindElementById(Nav.CatalogId);
        if (catalogTab != null)
        {
            await catalogTab.ClickAsync();
        }
        else
        {
            Assert.Fail("catalog tab not found!");
        }

        await action.WaitForSpinnerToDisappear();
    }

    #endregion

    #region Dataset

    public async Task OpenImportDatasetModalWindow(Wrapper action)
    {
        TestContext.Out.WriteLine("Open import modal window for dataset");

        var dropdown = await action.FindElementById(Ds.EditMask.CatalogMenuButtonId);
        if (dropdown != null)
        {
            await dropdown.ClickAsync();
        }
        else
        {
            Assert.Fail("catalog menu button not found!");
        }

        await action.WaitForSpinnerToDisappear();

        var importDatasetButton = await action.FindElementById(Ds.EditMask.ImportButtonId);
        if (importDatasetButton != null)
        {
            await importDatasetButton.ClickAsync();
        }
        else
        {
            Assert.Fail("import dataset button not found!");
        }

        await action.WaitForSpinnerToDisappear();
    }

    public async Task OpenCreateDatasetMask(Wrapper action)
    {
        TestContext.Out.WriteLine("Open create mask for dataset");

        var dropdown = await action.FindElementById(Ds.EditMask.CatalogMenuButtonId);
        if (dropdown != null)
        {
            await dropdown.ClickAsync();
        }
        else
        {
            Assert.Fail("catalog menu button not found!");
        }

        await action.WaitForSpinnerToDisappear();

        var createDatasetButton = await action.FindElementById(Ds.EditMask.CreateDatasetButtonId);
        if (createDatasetButton != null)
        {
            await createDatasetButton.ClickAsync();
        }
        else
        {
            Assert.Fail("create dataset button not found!");
        }

        await action.WaitForSpinnerToDisappear();
    }

    public async Task OpenEditDatasetMaskByName(Wrapper action, string titleDataset)
    {
        await action.WaitForSpinnerToDisappear();

        TestContext.Out.WriteLine($"Open edit mask for {titleDataset}");

        await SearchDatasetByName(action, titleDataset);

        await action.WaitForSpinnerToDisappear();

        await action.ClickButtonById(Ds.EditMask.CatalogTableViewButton + "0");
        await action.WaitForSpinnerToDisappear();

        await action.ClickTabsById(Ds.EditMask.TabDescriptionId);
        await action.WaitForSpinnerToDisappear();

        await action.ClickButtonById(Ds.EditMask.ButtonEditId);
        await action.WaitForSpinnerToDisappear();
    }

    public async Task OpenViewDatasetMaskByName(Wrapper action, string titleDataset)
    {
        TestContext.Out.WriteLine($"Open view mask for {titleDataset}");

        await SearchDatasetByName(action, titleDataset);

        await action.WaitForSpinnerToDisappear();

        await action.ClickButtonById(Ds.EditMask.CatalogTableViewButton + "0");
        await action.WaitForSpinnerToDisappear();

        await action.ClickTabsById(Ds.EditMask.TabDescriptionId);
        await action.WaitForSpinnerToDisappear();
    }

    public async Task OpenDistributionViewByDatasetName(Wrapper action, string titleDataset)
    {
        TestContext.Out.WriteLine("Open view for distribution for the generated dataset");

        await GoToDistributionListByDatasetName(action, titleDataset);

        var table = await action.FindElementById(Ds.EditMask.DistributionTableId);

        await action.WaitForSpinnerToDisappear();

        if (table != null)
        {
            var sum = await action.SumRowFromTable(Ds.EditMask.DistributionTableId);

            Assert.That(sum >= 1, Is.True, $"No distribution is found");

            if (sum > 0)
            {
                await action.ClickButtonById(Ds.EditMask.DistributionViewButtonRowId + "0");
                await action.WaitForSpinnerToDisappear();
            }
        }
        else
        {
            Assert.Fail("catalog-table not found");
        }
    }

    public async Task OpenDistributionEditByDatasetName(Wrapper action, string titleDataset)
    {
        TestContext.Out.WriteLine("Open edit mask for distribution for the generated dataset");

        await GoToDistributionListByDatasetName(action, titleDataset);

        var table = await action.FindElementById(Ds.EditMask.DistributionTableId);

        if (table != null)
        {
            var sum = await action.SumRowFromTable(Ds.EditMask.DistributionTableId);

            Assert.That(sum >= 1, Is.True, $"No distribution is found");

            if (sum > 0)
            {
                await action.ClickButtonById(Ds.EditMask.DistributionEditButtonRowId + "0");
                await action.WaitForSpinnerToDisappear();
            }
        }
        else
        {
            Assert.Fail("catalog-table not found");
        }
    }

    public async Task OpenDistributionCreateMaskByDatasetName(Wrapper action, string titleDataset)
    {
        TestContext.Out.WriteLine("Open view for distribution for the generated dataset");

        await GoToDistributionListByDatasetName(action, titleDataset);

        await action.ClickButtonById(Ds.EditMask.DistributionButtonId);

        await action.Wait1000();
    }

    public async Task GoToDistributionListByDatasetName(Wrapper action, string titleDataset)
    {
        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        TestContext.Out.WriteLine("Go to distribution-list for the generated dataset");

        await SearchDatasetByName(action, titleDataset);

        await action.WaitForSpinnerToDisappear();

        await action.ClickButtonById(Ds.EditMask.CatalogTableViewButton + "0");

        await action.WaitForSpinnerToDisappear();

        await action.Wait3500();

        await action.ClickTabsById(Ds.EditMask.TabDistributionId);

        await action.WaitForSpinnerToDisappear();
    }

    public async Task<int> SearchCountDatasetByName(Wrapper action, string identifier)
    {
        return await SearchAndCountResults(action, Concepts.CatalogSearchId, Concepts.CatalogTableId, identifier, "dataset");
    }

    public async Task SearchDatasetByName(Wrapper action, string titleDataset)
    {
        await SearchByCategory(action, Ds.EditMask.CatalogSearchId, Ds.EditMask.CatalogTableId, titleDataset, "dataset");
    }

    #endregion Dataset

    #region DataService

    public async Task OpenViewDataserviceMaskByName(Wrapper action, string titleDataservice)
    {
        TestContext.Out.WriteLine($"Open view mask for {titleDataservice}");

        await SearchDataServiceByName(action, titleDataservice);

        await action.WaitForSpinnerToDisappear();


        await action.ClickButtonById(Ds.EditMask.CatalogTableViewButton + "0");
        await action.WaitForSpinnerToDisappear();
    }

    public async Task OpenEditDataServiceMaskByName(Wrapper action, string titleDataService)
    {
        await action.WaitForSpinnerToDisappear();

        TestContext.Out.WriteLine($"Open edit mask for {titleDataService}");

        await SearchDataServiceByName(action, titleDataService);

        await action.WaitForSpinnerToDisappear();

        await action.ClickButtonById(Concepts.CatalogTableViewButton + "0");
        await action.WaitForSpinnerToDisappear();

        await action.ClickButtonById(DataServiceConstants.EditMask.ButtonEditId);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();
    }

    public async Task SearchDataServiceByName(Wrapper action, string titleDataService)
    {
        await SearchByCategory(action, Concepts.CatalogSearchId, Concepts.CatalogTableId, titleDataService, "data service");
    }

    #endregion

    #region Search

    public async Task SearchByCategory(Wrapper action, string searchId, string tableId, string searchTerm, string category)
    {
        TestContext.Out.WriteLine($"Search {searchTerm} in {category}");

        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        await action.SearchById(searchId, searchTerm);
        await action.WaitForSpinnerToDisappear();

        var table = await action.FindElementById(tableId);

        if (table != null)
        {
            var sum = await action.SumRowFromTable(tableId);
            Assert.That(sum >= 1, Is.True, $"A {category} was expected in the search '{searchTerm}', but none was found");
        }
        else
        {
            Assert.Fail($"{category} table not found");
        }
    }

    public async Task<int> SearchAndCountResults(Wrapper action, string searchId, string tableId, string searchTerm, string category)
    {
        TestContext.Out.WriteLine($"Search {searchTerm} in {category}");

        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        await action.SearchById(searchId, searchTerm);
        await action.WaitForSpinnerToDisappear();

        var result = await CountTableResults(action, tableId, category);

        return result;
    }

    public async Task<int> CountTableResults(Wrapper action, string tableId, string category)
    {
        var table = await action.FindElementById(tableId);

        if (table != null)
        {
            await action.Wait1000();
            return await action.SumRowFromTable(tableId);
        }
        else
        {
            Assert.Fail($"{category} table not found");
        }

        return 0;
    }

    /// <summary>
    /// Searches for a specific value in a table and returns its index.
    /// </summary>
    /// <param name="action">The wrapper containing table interaction methods</param>
    /// <param name="tableId">The ID of the table to search in</param>
    /// <param name="valueId">The base ID used for value elements in the table</param>
    /// <param name="expectedValue">The value to search for in the table</param>
    /// <param name="category">The table category used for error messages</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains matching (default: true)</param>
    /// <returns>
    /// The zero-based index of the first matching value in the table, or -1 if no match is found
    /// </returns>
    public async Task<int> SearchIndexFromTableValue(Wrapper action, string tableId, string valueId, string expectedValue, string category, bool? exactMatch = true)
    {

        var table = await action.FindElementById(tableId);

        if (table != null)
        {
            var length = await CountTableResults(action, tableId, category);

            if (length > 0)
            {
                var expectedNormalized = action.NormalizeText(expectedValue);

                for (int i = 0; i < length; i++)
                {
                    await action.ScrollIntoViewById(valueId + i.ToString());

                    var actualValue = await action.ReadDiv(valueId + i.ToString());
                    var actualNormalized = action.NormalizeText(actualValue);

                    var isFound = exactMatch ?? true
                                    ? expectedNormalized.Equals(actualNormalized)
                                    : actualValue.Contains(expectedNormalized);

                    if (isFound)
                    {
                        return i;
                    }
                    ;
                }
            }
        }
        else
        {
            Assert.Fail($"{category} table not found");
        }
        return -1;
    }

    #endregion Search

    #region PublicService

    public async Task OpenCreatePublicServicesMask(Wrapper action)
    {
        TestContext.Out.WriteLine("Open create mask for public services");

        var dropdown = await action.FindElementById(Ds.EditMask.CatalogMenuButtonId);
        if (dropdown != null)
        {
            await dropdown.ClickAsync();
        }
        else
        {
            Assert.Fail("catalog menu button not found!");
        }

        await action.WaitForSpinnerToDisappear();

        var createPublicServiceButton = await action.FindElementById(PublicServiceConstants.ViewMask.CreatePublicServiceButtonId);
        if (createPublicServiceButton != null)
        {
            await createPublicServiceButton.ClickAsync();
        }
        else
        {
            Assert.Fail("create dataset button not found!");
        }

        await action.WaitForSpinnerToDisappear();
    }

    public async Task SearchPublicServicesByName(Wrapper action, string identifier)
    {
        await SearchByCategory(action, PublicServiceConstants.ViewMask.CatalogSearchId, PublicServiceConstants.ViewMask.CatalogTableId, identifier, "public service");
    }

    public async Task OpenViewPublicServicesMaskByName(Wrapper action, string titlePublicServices)
    {
        TestContext.Out.WriteLine($"Open view mask for {titlePublicServices}");

        await SearchPublicServicesByName(action, titlePublicServices);

        await action.WaitForSpinnerToDisappear();

        await action.Wait1500();

        await action.ClickButtonById(PublicServiceConstants.ViewMask.CatalogTableViewButton + "0");
        await action.WaitForSpinnerToDisappear();

        await action.ClickTabsById(PublicServiceConstants.ViewMask.PublicServiceTabDescriptionId);
        await action.WaitForSpinnerToDisappear();
    }

    public async Task OpenEditPublicServicesMaskByName(Wrapper action, string titlePublicServices)
    {
        await OpenViewPublicServicesMaskByName(action, titlePublicServices);

        TestContext.Out.WriteLine($"Open Edit mask for {titlePublicServices}");

        await action.ClickTabsById(PublicServiceConstants.ViewMask.PublicServiceTabDescriptionId);

        await action.WaitForSpinnerToDisappear();

        await action.ClickTabsById(PublicServiceConstants.ViewMask.PublicServiceTabDescriptionId);

        await action.WaitForSpinnerToDisappear();

        await action.ClickTabsById(PublicServiceConstants.ViewMask.PublicServiceViewEditButtonId);

        await action.WaitForSpinnerToDisappear();
    }

    #endregion

    #region SaveAndClose

    public async Task SaveAndCloseDataset(Wrapper action)
    {
        TestContext.Out.WriteLine("Save and Close Dataset");

        var saveAndClose = await action.FindElementById(Ds.EditMask.DescriptionSaveAndCloseId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The minimum dataset could not be saved");

        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    public async Task SaveAndCloseDistribution(Wrapper action)
    {
        TestContext.Out.WriteLine("Save and Close Distribution");
        var saveAndClose = await action.FindElementById(Ds.EditMask.DistributionSaveAndCloseId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The distribution could not be saved");

        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    public async Task SaveAndCloseConcept(Wrapper action)
    {
        TestContext.Out.WriteLine("Save and Close Concept");

        var saveAndClose = await action.FindElementById(Concepts.ConceptSaveAndCloseId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The concept could not be saved");

        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    public async Task SaveAndCloseCodelistValue(Wrapper action)
    {
        await TestContext.Out.WriteLineAsync("Save and Close Codelist Value");

        var saveAndClose = await action.FindElementById(Concepts.CodelistSumitButtonId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The concept could not be saved");

        await action.WaitForInputValidationById(Concepts.CodelistValueId);
        await action.WaitForInputValidationById(Concepts.CodelistParentId);
        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    public async Task SaveAndCloseDataService(Wrapper action)
    {
        TestContext.Out.WriteLine("Save and Close DataService");

        var saveAndClose = await action.FindElementById(DataServiceConstants.EditMask.DescriptionSaveAndCloseId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The data service could not be saved");

        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    public async Task SaveAndClosePublicService(Wrapper action)
    {
        TestContext.Out.WriteLine("Save and Close PublicServices");

        var saveAndClose = await action.FindElementById(PublicServiceConstants.EditMask.DescriptionSaveAndCloseId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The public service could not be saved");

        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    public async Task SaveAndCloseAnnotation(Wrapper action)
    {
        TestContext.Out.WriteLine("Save and Close Annotation");

        var saveAndClose = await action.FindElementById(Concepts.EditAnnotationSaveButtonId);

        Assert.That(await action.IsDisabled(saveAndClose), Is.False, "The annotation could not be saved");

        await saveAndClose!.ClickAsync();
        await action.WaitForSpinnerToDisappear();
    }

    #endregion

    #region Concept

    public async Task OpenCreateConceptMask(Wrapper action)
    {
        TestContext.Out.WriteLine("Open create mask for concept");
        await action.Wait500();
        await action.WaitForSpinnerToDisappear();
        var dropdown = await action.FindElementById(Concepts.CatalogMenuButtonId);

        if (dropdown != null)
        {
            await dropdown.ClickAsync();

            await action.Wait500();
        }
        else
        {
            Assert.Fail("catalog menu button not found!");
        }

        await action.WaitForSpinnerToDisappear();

        var createConceptButton = await action.FindElementById(Concepts.CreateConceptButtonId);
        if (createConceptButton != null)
        {
            await createConceptButton.ClickAsync();

            await action.WaitForSpinnerToDisappear();
        }
        else
        {
            Assert.Fail("catalog create button not found!");
        }
    }

    public async Task<int> SearchCountConceptByName(Wrapper action, string identifier)
    {
        return await SearchAndCountResults(action, Concepts.CatalogSearchId, Concepts.CatalogTableId, identifier, "concept");
    }

    public async Task SearchConceptByName(Wrapper action, string identifier)
    {
        await SearchByCategory(action, Concepts.CatalogSearchId, Concepts.CatalogTableId, identifier, "concept");
    }

    public async Task DeleteConcept(Wrapper action)
    {

        await action.ScrollIntoViewById(Concepts.ConceptDeleteId);
        await action.ClickButtonById(Concepts.ConceptDeleteId);
        await action.Wait500();
    }

    #endregion

    #region PublicationLevel

    public async Task SetPublicLevelProposalToPublic(Wrapper action)
    {
        TestContext.Out.WriteLine("Set Status to Candidate");
        await action.ScrollIntoViewById(StatusLevelVersion.AllowedStatusButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedStatusButtonId);
        await action.Wait1000();
        await action.ClickButtonById(StatusLevelVersion.AllowedStatusId + StatusLevelVersion.Candidate);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        TestContext.Out.WriteLine("Set Level Proposal to Public");
        await action.ScrollIntoViewById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        await action.ClickButtonById(StatusLevelVersion.AllowedLevelsId + StatusLevelVersion.Public);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();
    }

    public async Task SetPublicLevelToPublic(Wrapper action)
    {
        TestContext.Out.WriteLine("Set Status to Recorded");
        await action.ScrollIntoViewById(StatusLevelVersion.AllowedStatusButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedStatusButtonId);
        await action.Wait1000();

        await action.ClickButtonById(StatusLevelVersion.AllowedStatusId + StatusLevelVersion.Recorded);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        TestContext.Out.WriteLine("Set Level to Public");
        await action.ScrollIntoViewById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        await action.ScrollIntoViewById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedLevelsId + StatusLevelVersion.Public);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();
    }

    public async Task ResetPublicLevelToInternal(Wrapper action)
    {
        TestContext.Out.WriteLine("Set Status to Incomplete");
        await action.ScrollIntoViewById(StatusLevelVersion.AllowedStatusButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedStatusButtonId);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        await action.ClickButtonById(StatusLevelVersion.AllowedStatusId + StatusLevelVersion.Incomplete);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        TestContext.Out.WriteLine("Set Level to Unit");
        await action.ScrollIntoViewById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.ClickButtonById(StatusLevelVersion.AllowedLevelsButtonId);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();

        await action.ClickButtonById(StatusLevelVersion.AllowedLevelsId + StatusLevelVersion.Unit);
        await action.WaitForSpinnerToDisappear();
        await action.Wait1000();
    }

    #endregion

    #region div

    /// <summary>
    /// Validates whether the content of an HTML div element matches an expected value.
    /// </summary>
    /// <param name="action">A Wrapper instance for Playwright interactions</param>
    /// <param name="name">The name of the property being checked (for test output)</param>
    /// <param name="id">The ID of the HTML div element to check</param>
    /// <param name="value">The expected content value for comparison</param>
    /// <param name="strict">
    /// Optional. Determines the type of comparison:
    /// - true (default): Checks for exact match between expected and actual value
    /// - false: Checks if the actual value contains the expected value
    /// </param>
    /// <exception cref="AssertionException">Thrown when the comparison fails</exception>
    public async Task CheckDivPropertyById(Wrapper action, string name, string id, string value, bool strict = true)
    {
        TestContext.Out.WriteLine($"Checks whether the {name} has been implemented correctly");
        await action.ScrollOnTopById(id);

        var res = await action.ValidateDivContent(id, value, strict);
        Assert.That(res, Is.True, $"{name} not congruent with expected value");
    }

    public async Task<string> ReadChip(Wrapper action, string chipId)
    {
        var spanElement = await action.FindNestedElement(chipId, AttributesAndElements.ChipSpan);
        if (spanElement == null)
        {
            throw new Exception($"Chip text element not found for chip {chipId}");
        }

        return await spanElement.TextContentAsync() + string.Empty;
    }

    public async Task<string?> ReadDiv(Wrapper action, string elementId)
    {
        await action.WaitForElementToBeStable(elementId);
        var divElement = await action.FindElementById(elementId);

        if (divElement != null)
        {
            return await divElement.TextContentAsync();
        }

        return null;
    }

    #endregion
}
