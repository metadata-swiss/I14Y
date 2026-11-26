using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Shared;
using Bfs.Iop.Test.Abstraction.Helpers;

namespace Bfs.Iop.Admin.Testautomation.Helpers;

internal static class ConceptEditMaskHelper
{
    private const int MaxSearchRetries = 3;

    /// <summary>
    /// Searches the concept in the catalog and opens its detail page.
    /// Waits for the matching title so stale search results cannot open another concept.
    /// </summary>
    public static async Task GoToConceptDetail(
        Wrapper actions,
        StandardTask standardAction,
        string identifier)
    {
        await SearchConceptsByTitle(actions, standardAction, identifier);
        await actions.OpenTableRowWithTitle(Concepts.CatalogTableId, identifier);
        await actions.WaitForElementToBeStable(StatusLevelVersion.ChipStatusId);
    }

    public static async Task<int> SearchConceptsByTitle(Wrapper actions, StandardTask standardAction, string identifier)
    {
        await standardAction.GotoCatalog(actions);

        for (var retry = 0; retry <= MaxSearchRetries; retry++)
        {
            var count = await actions.SearchCatalogRowsWithTitle(Concepts.CatalogSearchId, Concepts.CatalogTableId, identifier);
            if (count > 0)
            {
                return count;
            }

            if (retry < MaxSearchRetries)
            {
                TestContext.Out.WriteLine($"Concept {identifier} not indexed yet, retry {retry + 1}/{MaxSearchRetries} in 3 seconds.");
                await actions.Wait3000();
            }
        }

        Assert.Fail($"The concept {identifier} was not found in the catalog.");
        return 0;
    }

    /// <summary>
    /// Fills the mandatory fields of the concept edit mask. The concept type is set with one of the Set...Type methods.
    /// </summary>
    public static async Task FillConceptMinimal(
        Wrapper actions,
        string name,
        string description,
        string identifier,
        string validFrom)
    {
        await actions.ScrollIntoViewById(Concepts.NameDeId);
        TestContext.Out.WriteLine("Write Name");
        await actions.FillInputAndEnterById(Concepts.NameDeId, name);

        await actions.ScrollIntoViewById(Concepts.DescriptionDeId);
        TestContext.Out.WriteLine("Write Description");
        await actions.FillInputById(Concepts.DescriptionDeId, description);

        await actions.ScrollIntoViewById(Concepts.IdentifierId);
        TestContext.Out.WriteLine($"Write Identifier: {identifier}");
        await actions.FillInputById(Concepts.IdentifierId, identifier);

        await actions.ScrollIntoViewById(Concepts.PublisherId);
        TestContext.Out.WriteLine($"Write Publisher: {Concepts.PublisherId}={Concepts.PublisherIdOptionTestOrganisation}");
        await actions.SelectOptionById(Concepts.PublisherId, Concepts.PublisherIdOptionTestOrganisation);

        await actions.ScrollIntoViewById(Concepts.ResponsiblePersonId);
        TestContext.Out.WriteLine("Write Responsible Person");
        await actions.SelectFirstAutocompleteById(Concepts.ResponsiblePersonId, ConceptData.ResponsiblePersonName);

        await actions.ScrollIntoViewById(Concepts.ValidFromId);
        TestContext.Out.WriteLine("Write ValidFrom Date");
        await actions.FillInputAndEnterById(Concepts.ValidFromId, validFrom);

        await actions.ScrollIntoViewById(Concepts.ResponsibleDeputyId);
        TestContext.Out.WriteLine("Write Responsible Deputy");
        await actions.SelectFirstAutocompleteById(Concepts.ResponsibleDeputyId, ConceptData.DeputyPersonName);
    }

    public static async Task SetStringType(Wrapper actions)
    {
        await actions.ScrollIntoViewById(Concepts.ConceptTypeId);
        TestContext.Out.WriteLine("Write String Type");
        await actions.SelectOptionById(Concepts.ConceptTypeId, Concepts.ConceptTypeOptionStringId);

        await actions.ScrollIntoViewById(Concepts.MinLengthId);
        TestContext.Out.WriteLine("Write MinLength");
        await actions.FillInputById(Concepts.MinLengthId, "1");

        await actions.ScrollIntoViewById(Concepts.MaxLengthId);
        TestContext.Out.WriteLine("Write MaxLength");
        await actions.FillInputById(Concepts.MaxLengthId, "1024");
    }

    public static async Task SetNumericType(Wrapper actions)
    {
        await actions.ScrollIntoViewById(Concepts.ConceptTypeId);
        TestContext.Out.WriteLine("Write Numeric Type");
        await actions.SelectOptionById(Concepts.ConceptTypeId, Concepts.ConceptTypeOptionNumericId);

        await actions.ScrollIntoViewById(Concepts.NbDecimalId);
        TestContext.Out.WriteLine("Write NbDecimal");
        await actions.FillInputById(Concepts.NbDecimalId, "0");

        await actions.ScrollIntoViewById(Concepts.MinValueId);
        TestContext.Out.WriteLine("Write MinValue");
        await actions.FillInputById(Concepts.MinValueId, "1");

        await actions.ScrollIntoViewById(Concepts.MaxValueId);
        TestContext.Out.WriteLine("Write MaxValue");
        await actions.FillInputById(Concepts.MaxValueId, "1024");
    }

    public static async Task SetDateType(Wrapper actions)
    {
        await actions.ScrollIntoViewById(Concepts.ConceptTypeId);
        TestContext.Out.WriteLine("Write Date Type");
        await actions.SelectOptionById(Concepts.ConceptTypeId, Concepts.ConceptTypeOptionDateId);

        await actions.ScrollIntoViewById(Concepts.PatternDateId);
        TestContext.Out.WriteLine("Write Pattern");
        await actions.FillInputById(Concepts.PatternDateId, "dd.MM.yy");
    }

    public static async Task SetCodelistType(Wrapper actions)
    {
        await actions.ScrollIntoViewById(Concepts.ConceptTypeId);
        TestContext.Out.WriteLine("Write Codelist Type");
        await actions.SelectOptionById(Concepts.ConceptTypeId, Concepts.ConceptTypeOptionCodeListId);

        await actions.ScrollIntoViewById(Concepts.CodelistEntryValueMaxLengthId);
        TestContext.Out.WriteLine("Write CodelistEntryValueMaxLength");
        await actions.FillInputById(Concepts.CodelistEntryValueMaxLengthId, "2024");

        await actions.ScrollIntoViewById(Concepts.CodeListEntryValueTypeId);
        TestContext.Out.WriteLine("Write CodeListEntryValueType");
        await actions.SelectOptionById(Concepts.CodeListEntryValueTypeId, Concepts.CodelistEntryOptionStringId);
    }
}
