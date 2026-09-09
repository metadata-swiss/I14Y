using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Helpers;
using Bfs.Iop.Admin.Testautomation.Models;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;
using System.Text.Json;
using MyShare = Bfs.Iop.Admin.Testautomation.Shared;

namespace Bfs.Iop.Admin.Testautomation.ExportImportCodelistEntryTests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class ExportImportCodelistentryTest : PlaywrightSetup
{
    private const string _codeList = "CodeList";
    private const string _secondPrefix = "Second";

    private StandardTask? _standardAction;
    private Listener? _listener;

    private string _nameConceptCodeList;
    private string _identifierConceptCodeList;
    private string _descriptionConceptCodeList;

    private string _nameConceptCodeList2;
    private string _identifierConceptCodeList2;
    private string _descriptionConceptCodeList2;

    private string _downloadPath;

    [SetUp]
    public void SetupInternal()
    {
        _nameConceptCodeList = ConceptData.NameConcept + _codeList + TimeStamp;
        _identifierConceptCodeList = ConceptData.IdentificatorConcept + _codeList + TimeStamp;
        _descriptionConceptCodeList = ConceptData.DescriptionConcept + TimeStamp;

        _nameConceptCodeList2 = _nameConceptCodeList + _secondPrefix;
        _identifierConceptCodeList2 = _identifierConceptCodeList + _secondPrefix;
        _descriptionConceptCodeList2 = _descriptionConceptCodeList + _secondPrefix;

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
    /// Creates the first minimal concept with basic data.
    /// </summary>
    [Test, Order(1)]
    public async Task ShouldCreateFirstMinimalConceptSuccessfully()
    {
        TestContext.Out.WriteLine($"create Concept {_identifierConceptCodeList2}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenCreateConceptMask(Actions);

        await CreateMinimalConcept(true);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseConcept(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        await Actions.WaitForSpinnerToDisappear();

        CheckApiErrror();
    }

    /// <summary>
    /// Define a second minimal concept with the same structure but a different prefix.
    /// </summary>
    [Test, Order(2)]
    public async Task ShouldCreateSecondMinimalConceptSuccessfully()
    {
        TestContext.Out.WriteLine($"create Concept {_identifierConceptCodeList}");

        await _standardAction!.ChangeLanguageToGerman(Actions);

        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.OpenCreateConceptMask(Actions);

        await CreateMinimalConcept(false);

        var urlTracker = new PageUrlTracker(Page);

        await _standardAction!.SaveAndCloseConcept(Actions);

        Assert.That(urlTracker.HasChanged(Page), Is.True, "The page was not saved and closed.");

        await Actions.WaitForSpinnerToDisappear();

        CheckApiErrror();
    }

    /// <summary>
    /// Open the editing mask for the first concept and add six code list entries one after the other:
    /// 1.)   One main entry(“Plutonite”) including annotation(type, title, identifier, text, URI).
    /// 2.)   Five child entries, each referencing the main entry as its parent.
    /// </summary>
    [Test, Order(3)]
    public async Task ShouldCreateCodelistSuccessfully()
    {
        var title = _identifierConceptCodeList;

        TestContext.Out.WriteLine($"create CodeList {title}");

        await OpenEditConceptMaskByName(title);

        await CreateCodelist();
    }

    /// <summary>
    /// Switch to the detailed view of the concept, click on “Export” and download the code list as JSON.
    /// </summary>
    [Test, Order(4)]
    public async Task ShouldExportCodelistSuccessfully()
    {
        var title =  _identifierConceptCodeList;

        TestContext.Out.WriteLine($"export CodeList {title}");

        await GoToConceptDetail(title);

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(Concepts.NavTreeSectionProperiesId);
        await Actions.WaitForSpinnerToDisappear();

        await Actions.ScrollIntoViewById(ExportImportCodelistentries.CodelistDownloadButtonId);

        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(ExportImportCodelistentries.CodelistDownloadButtonId);

        await Actions.WaitForSpinnerToDisappear();

        _downloadPath = await Actions.ClickDownloadById(ExportImportCodelistentries.CodelistDownloadJsonId);

        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear();

        await Actions.PressKey(Keys.Escape);

        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
        _listener.ResetErrors();

        await ShouldDoubleCheckDownloadSuccessfully();
    }

    /// <summary>
    /// Open the editing mask for the second concept and import the previously exported JSON file. The system then checks for missing API errors.
    /// </summary>
    [Test, Order(5)]
    public async Task ShouldDoubleCheckDownloadSuccessfully()
    {      
        string content = await File.ReadAllTextAsync(_downloadPath);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        //var model = JsonSerializer.Deserialize<CodelistDataModel>(content, options);
        var model = JsonSerializer.Deserialize<DataWrapper<IEnumerable<CodeListEntryInputModel>>>(content, options);

        if (model != null && model.Data != null && model.Data.Any())
        {
            DoubleCheckData(model.Data);
        }
        else
        {
            TestContext.Out.WriteLine($"{nameof(DataWrapper<IEnumerable<CodeListEntryInputModel>>)} could not be deserialized");
            Assert.IsFalse(true);
        }
    }

    /// <summary>
    /// Delete all versions of the first concept using the detail view and the confirmation dialog until no more entries are found in the catalog.
    /// </summary>
    [Test, Order(6)]
    public async Task ShouldImportCodelistSuccessfully()
    {
        TestContext.Out.WriteLine($"Import CodeList {_identifierConceptCodeList2}");

        await OpenEditConceptMaskByName(_identifierConceptCodeList2);

        await Actions.ScrollIntoViewById(ExportImportCodelistentries.CodelistImportButtonId);
        await Actions.WaitForSpinnerToDisappear();


        await Actions.SetInputFileByCssSelector(ExportImportCodelistentries.CodelistImportCssSeletor,  _downloadPath);

        await Actions.PressKey(Keys.Enter);

        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear();


        Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
        _listener.ResetErrors();

        await Actions.PressKey(Keys.Escape);
    }

    /// <summary>
    /// 
    /// </summary>
    [Test, Order(8)]
    public async Task ShouldDeleteConceptSuccessfully()
    {
        await DeleteConceptMaskByName();
    }

    private async Task DeleteConceptMaskByName()
    {
        TestContext.Out.WriteLine($"Delete Concept {_identifierConceptCodeList}");

        await Actions.Wait500();

        await _standardAction!.GotoCatalog(Actions);

        await Actions.WaitForSpinnerToDisappear();



        var count = await _standardAction!.SearchCountConceptByName(Actions, _identifierConceptCodeList);
        var customized = 1;

        await Actions.ClickButtonById(Concepts.CatalogTableViewButton + "0");

        await Actions.WaitForSpinnerToDisappear();
        await Actions.WaitForNotificationToDisappear();

        while (count != 0)
        {

            TestContext.Out.WriteLine($"Delete {customized}. version of the same  concept.");

            await GoToConceptDetail(_identifierConceptCodeList);
            await Actions.WaitForSpinnerToDisappear();
            await Actions.WaitForNotificationToDisappear();

            await Actions.Wait1000();

            await Actions.ClickButtonById(Concepts.ConceptDeleteId);
            await Actions.Wait500();

            await Actions.ClickButtonById(Test.Abstraction.Constants.Dialogs.ConfirmId);

            await Actions.WaitForSpinnerToDisappear();
            await Actions.WaitForNotificationToDisappear();
            await Actions.Wait1000();

            // TODO: Must be shut down because the front end is making a debricate API query.

            // Assert.That(_listener!.HasApiErrors(), Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
            _listener.ResetErrors();
            count--;
            customized++;
        }
    }

    private async Task OpenEditConceptMaskByName(string title)
    {
        TestContext.Out.WriteLine("Open view mask");

        await GoToConceptDetail(title);
        await Actions.WaitForSpinnerToDisappear();

        await Actions.Wait1000();
        TestContext.Out.WriteLine("Open edit mask");

        await Actions.ScrollIntoViewById(Concepts.ConceptEditId);
        await Actions.ClickButtonById(Concepts.ConceptEditId);
        await Actions.WaitForSpinnerToDisappear();
    }

    private async Task GoToConceptDetail(string title)
    {
        await _standardAction!.GotoCatalog(Actions);

        await _standardAction!.SearchConceptByName(Actions, title);
        await Actions.WaitForSpinnerToDisappear();

        await Actions.ClickButtonById(Concepts.CatalogTableViewButton + "0");
        await Actions.WaitForSpinnerToDisappear();
    }

    private async Task CreateMinimalConcept(bool firstConcept)
    {
        var identificator = _identifierConceptCodeList;
        var description = _descriptionConceptCodeList;
        var name = _nameConceptCodeList;

        if (!firstConcept)
        {
            identificator = _identifierConceptCodeList2;
            description = _descriptionConceptCodeList2;
            name = _nameConceptCodeList2;
        }

        await Actions.ScrollIntoViewById(Concepts.NameDeId);
        TestContext.Out.WriteLine("Write Name");
        await Actions.FillInputAndEnterById(Concepts.NameDeId, name);

        await Actions.ScrollIntoViewById(Concepts.DescriptionDeId);
        TestContext.Out.WriteLine("Write Description");
        await Actions.FillInputById(Concepts.DescriptionDeId, description);

        await Actions.ScrollIntoViewById(Concepts.IdentifierId);
        TestContext.Out.WriteLine($"Write Identifier: {identificator}");
        await Actions.FillInputById(Concepts.IdentifierId, identificator!);

        await Actions.ScrollIntoViewById(Concepts.PublisherId);
        TestContext.Out.WriteLine($"Write Publisher: {Concepts.PublisherId}={Concepts.PublisherIdOptionTestOrganisation}");
        await Actions.SelectOptionById(Concepts.PublisherId, Concepts.PublisherIdOptionTestOrganisation);

        await Actions.ScrollIntoViewById(Concepts.ResponsiblePersonId);
        TestContext.Out.WriteLine("Write Responsible Person");
        await Actions.SelectFirstAutocompleteById(Concepts.ResponsiblePersonId, ConceptData.ResponsiblePersonName);

        await Actions.ScrollIntoViewById(Concepts.ValidFromId);
        TestContext.Out.WriteLine("Write ValidFrom Date");
        await Actions.FillInputAndEnterById(Concepts.ValidFromId, CurrentDate);

        await Actions.ScrollIntoViewById(Concepts.ResponsibleDeputyId);
        TestContext.Out.WriteLine("Write Responsible Person");
        await Actions.SelectFirstAutocompleteById(Concepts.ResponsibleDeputyId, ConceptData.DeputyPersonName);

        await Actions.ScrollIntoViewById(Concepts.ConceptTypeId);
        TestContext.Out.WriteLine("Write Codelist Type");
        await Actions.SelectOptionById(Concepts.ConceptTypeId, Concepts.ConceptTypeOptionCodeListId);

        await Actions.ScrollIntoViewById(Concepts.CodelistEntryValueMaxLengthId);
        TestContext.Out.WriteLine("Write CodelistEntryValueMaxLength");
        await Actions.FillInputById(Concepts.CodelistEntryValueMaxLengthId, "2024");

        await Actions.ScrollIntoViewById(Concepts.CodeListEntryValueTypeId);
        TestContext.Out.WriteLine("Write CodeListEntryValueType");
        await Actions.SelectOptionById(Concepts.CodeListEntryValueTypeId, Concepts.CodelistEntryOptionStringId);
    }

    private async Task CreateCodelist()
    {
        await Actions.Wait500();
        TestContext.Out.WriteLine("Write Codelist Value");

        var childValues = new[]
        {
            ConceptData.CodelistValue1,
            ConceptData.CodelistValue2,
            ConceptData.CodelistValue3,
            ConceptData.CodelistValue4,
            ConceptData.CodelistValue5,
        };

        // First
        await AddNewCodelistMask();

        await Actions.ScrollIntoViewById(Concepts.CodelistValueId);
        await Actions.FillInputById(Concepts.CodelistValueId, ConceptData.CodelistValue);

        await Actions.ScrollIntoViewById(Concepts.CodelistCodeNameDeId);
        await Actions.FillInputById(Concepts.CodelistCodeNameDeId, ConceptData.CodelistNameDe);

        await Actions.ScrollIntoViewById(Concepts.CodelistDescriptionDeId);
        await Actions.FillInputById(Concepts.CodelistDescriptionDeId, ConceptData.CodelistDescriptionDe);

        await _standardAction!.SaveAndCloseCodelistValue(Actions);

        TestContext.Out.WriteLine("Creating annotation for main entry");
        await CreateAnnotation();
        TestContext.Out.WriteLine($"✓ Main entry '{ConceptData.CodelistValue}' with annotation created");

        // Create children:
        foreach (var item in childValues)
        {
            await AddNewCodelistMask();

            await Actions.ScrollIntoViewById(Concepts.CodelistValueId);
            await Actions.FillInputById(Concepts.CodelistValueId, item);
            await Actions.SelectFirstAutocompleteById(Concepts.CodelistParentId, ConceptData.CodelistValue);
            await Actions.ScrollIntoViewById(Concepts.CodelistCodeNameDeId);
            await Actions.FillInputById(Concepts.CodelistCodeNameDeId, item);

            await _standardAction!.SaveAndCloseCodelistValue(Actions);

            await Actions.Wait500();
        }

        await Actions.Wait500();
    }

    private async Task CreateAnnotation()
    {
        await Actions.ScrollIntoViewById(Concepts.ShowAnnotationGridId + "0");
        await Actions.ClickButtonById(Concepts.ShowAnnotationGridId + "0");
        await Actions.WaitForSpinnerToDisappear();

        await Actions.ScrollPageDown(Wrapper.ScrollMethod.KeyboardScroll);
        await Actions.WaitForSpinnerToDisappear();

        await Actions.ScrollIntoViewById(Concepts.AddAnnotationRowId);
        await Actions.ClickButtonById(Concepts.AddAnnotationRowId);

        await FillAnnotation();

        await _standardAction!.SaveAndCloseAnnotation(Actions);
    }

    private async Task FillAnnotation()
    {
        TestContext.Out.WriteLine("Starting Annotation Creation");

        TestContext.Out.WriteLine($"Entering annotation type: {ConceptData.AnnotationType}");
        await Actions.FillInputById(Concepts.EditAnnotationTypeId, ConceptData.AnnotationType);

        TestContext.Out.WriteLine($"Entering annotation title: {ConceptData.AnnotationTitle}");
        await Actions.FillInputById(Concepts.EditAnnotationTitleId, ConceptData.AnnotationTitle);

        TestContext.Out.WriteLine($"Entering annotation identifier: {ConceptData.AnnotationIdentifier}");
        await Actions.FillInputById(Concepts.EditAnnotationIdentifierId, ConceptData.AnnotationIdentifier);

        TestContext.Out.WriteLine("Entering annotation text (DE)");
        await Actions.FillInputById(Concepts.EditAnnotationTextDeId, ConceptData.AnnotationTextDe);

        TestContext.Out.WriteLine($"Entering annotation URI: {ConceptData.AnnotationUri}");
        await Actions.FillInputById(Concepts.EditAnnotationUriId, ConceptData.AnnotationUri);

        TestContext.Out.WriteLine("✓ Annotation data entry completed");
    }

    private async Task AddNewCodelistMask()
    {
        await Actions.WaitForNotificationToDisappear();
        await Actions.WaitForSpinnerToDisappear();
        await Actions.ScrollOnTopByControlName(Concepts.ControlNameCodeList);
        await Actions.Wait500();
        await Actions.ScrollIntoViewById(Concepts.CodelistAddRowId);
        await Actions.ClickButtonById(Concepts.CodelistAddRowId);
        await Actions.Wait1000();

    }

    private void CheckApiErrror()
    {
        var hasError = _listener!.HasApiErrors();
        Assert.That(hasError, Is.False, $"API errors occurred during test execution /n{_listener!.GetLastErrorMessage}");
        if (hasError)
        {
            _listener.ResetErrors();
        }
    }

    private void DoubleCheckData(IEnumerable<CodeListEntryInputModel> data)
    {
        TestContext.Out.WriteLine("Starting Codelist Model Validation");

        // Basic structure validation
        TestContext.Out.WriteLine("Checking basic model structure");
        Assert.That(data, Is.Not.Null, "Data should not be null");
        TestContext.Out.WriteLine("✓ Data list exists");

        var codeListEntriesCount = data.Count();
        Assert.That(codeListEntriesCount, Is.EqualTo(6), "There should be exactly 6 Codelist entries.");
        TestContext.Out.WriteLine($"✓ Data contains the expected number of entries: {codeListEntriesCount}");

        // Validate main Plutonite entry
        TestContext.Out.WriteLine($"Validating main entry: {ConceptData.CodelistValue}");
        var plutoniteEntry = data.FirstOrDefault(item => item.Code == ConceptData.CodelistValue);
        Assert.That(plutoniteEntry, Is.Not.Null, $"The entry with the code '{ConceptData.CodelistValue}' should exist.");
        TestContext.Out.WriteLine($"✓ Main entry found: {ConceptData.CodelistValue}");

        // Validate main entry properties
        Assert.That(plutoniteEntry.Name?.De, Is.EqualTo(ConceptData.CodelistNameDe), "The German name for plutonite is incorrect.");
        TestContext.Out.WriteLine($"✓ Name validation passed: {plutoniteEntry.Name?.De}");

        Assert.That(plutoniteEntry.Description?.De, Is.EqualTo(ConceptData.CodelistDescriptionDe), "The German description for plutonite is incorrect");
        TestContext.Out.WriteLine("✓ Description validation passed");

        // Annotation validation
        var annotationsCount = plutoniteEntry.Annotations.Count();
        TestContext.Out.WriteLine("Validating annotation for main entry");
        Assert.IsTrue(annotationsCount > 0, "Plutonite should have at least one annotation");
        TestContext.Out.WriteLine($"✓ Annotation count: {annotationsCount}");

        var annotation = plutoniteEntry.Annotations.First();

        Assert.That(annotation.Identifier, Is.EqualTo(ConceptData.AnnotationIdentifier), "The annotation ID is incorrect.");
        TestContext.Out.WriteLine($"✓ Annotation identifier validated: {annotation.Identifier}");

        Assert.That(annotation.Title, Is.EqualTo(ConceptData.AnnotationTitle), "The annotation title is incorrect.");
        TestContext.Out.WriteLine($"✓ Annotation title validated: {annotation.Title}");

        Assert.That(annotation.Type, Is.EqualTo(ConceptData.AnnotationType), "The annotation type is incorrect.");
        TestContext.Out.WriteLine($"✓ Annotation type validated: {annotation.Type}");

        Assert.That(annotation.Uri, Is.EqualTo(ConceptData.AnnotationUri), "The annotation URI is incorrect");
        TestContext.Out.WriteLine($"✓ Annotation URI validated: {annotation.Uri}");

        Assert.That(annotation.Text?.De, Is.EqualTo(ConceptData.AnnotationTextDe), "The German annotation text is incorrect.");
        TestContext.Out.WriteLine("✓ Annotation text validated");

        // Validate child entries
        TestContext.Out.WriteLine("Validating child entries");
        CheckChildEntry(data, ConceptData.CodelistValue1, ConceptData.CodelistValue);
        CheckChildEntry(data, ConceptData.CodelistValue2, ConceptData.CodelistValue);
        CheckChildEntry(data, ConceptData.CodelistValue3, ConceptData.CodelistValue);
        CheckChildEntry(data, ConceptData.CodelistValue4, ConceptData.CodelistValue);
        CheckChildEntry(data, ConceptData.CodelistValue5, ConceptData.CodelistValue);

        TestContext.Out.WriteLine("✓ All child entries validated successfully");
    }

    private static void CheckChildEntry(IEnumerable<CodeListEntryInputModel> data, string childCode, string parentCode)
    {
        TestContext.Out.WriteLine($"Validating child entry: {childCode}");

        var childEntry = data.FirstOrDefault(item => item.Code == childCode);
        Assert.That(childEntry, Is.Not.Null, $"The entry with the code '{childCode}' should exist.");
        TestContext.Out.WriteLine($"✓ Child entry found: {childCode}");

        Assert.That(childEntry.ParentCode, Is.EqualTo(parentCode), $"The parent code for '{childCode}' should be '{parentCode}'.");
        TestContext.Out.WriteLine($"✓ Parent relationship validated: {childCode} → {parentCode}");

        Assert.That(childEntry.Name?.De, Is.EqualTo(childCode), $"The German name for '{childCode}' is incorrect.");
        TestContext.Out.WriteLine($"✓ Name validation passed: {childEntry.Name?.De}");

        Assert.That(childEntry.Annotations.Count(), Is.EqualTo(0), $"'{childCode}' should not have any annotations");
        TestContext.Out.WriteLine($"✓ Confirmed {childCode} has no annotations");

        TestContext.Out.WriteLine($"✓ Child entry '{childCode}' validated successfully");
    }
}
