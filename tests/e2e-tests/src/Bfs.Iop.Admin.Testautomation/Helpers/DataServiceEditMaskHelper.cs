using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Test.Abstraction.Extensions;
using Bfs.Iop.Test.Abstraction.Helpers;

namespace Bfs.Iop.Admin.Testautomation.Helpers;

internal static class DataServiceEditMaskHelper
{
    public static async Task FillDataServiceMinimal(
        Wrapper actions,
        string title,
        string identifier)
    {
        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.TitleDeId);
        TestContext.Out.WriteLine("Write DataService Title");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.TitleDeId, title);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.DescriptionDeId);
        TestContext.Out.WriteLine("Write DataService Description");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.DescriptionDeId, identifier);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.PublisherId);
        TestContext.Out.WriteLine("Write Publisher");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.PublisherId, DataServiceConstants.Data.PublisherViewDe);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.AccessRightsId);
        TestContext.Out.WriteLine("Write AccessRights");
        await actions.SelectOptionById(DataServiceConstants.EditMask.AccessRightsId, DataServiceConstants.EditMask.AccessRightsOptionId);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.ContactPointEmailId);
        TestContext.Out.WriteLine("Write Contact Point Email");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.ContactPointEmailId, DataServiceConstants.Data.ContactPointEmailValue);

        TestContext.Out.WriteLine("Write Endpoint URL");
        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.EndpointUrlsControlName);
        var itemEndpointURL = new KeyValuePair<string, string>[] { new(DataServiceConstants.EditMask.EndpointUrlsTextId, DataServiceConstants.Data.HrefIdValue) };
        await actions.FillEditTableRow(DataServiceConstants.EditMask.EndpointUrlsAddRowId, DataServiceConstants.EditMask.EndpointUrlsSaveRowId, 0, itemEndpointURL);
    }

    public static async Task<IReadOnlyDictionary<string, string>> FillToUpdateDataServiceMaximal(
        Wrapper actions)
    {
        var usedVocabularyValues = new Dictionary<string, string>();

        TestContext.Out.WriteLine("Write Endpoint Description");
        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.EndpointDescriptionControlName);
        var itemEndpointDescription = new KeyValuePair<string, string>[] { new(DataServiceConstants.EditMask.EndpointDescriptionTextId, DataServiceConstants.Data.HrefIdValue) };
        await actions.FillEditTableRow(DataServiceConstants.EditMask.EndpointDescriptionAddRowId, DataServiceConstants.EditMask.EndpointDescriptionSaveRowId, 0, itemEndpointDescription);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.ContactPointOrganisationId);
        TestContext.Out.WriteLine("Write Contact point Organisation");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.ContactPointOrganisationId, DataServiceConstants.Data.ContactOrganisationValue);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.ContactPointAddressId);
        TestContext.Out.WriteLine("Write Contact point Address");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.ContactPointAddressId, DataServiceConstants.Data.ContactAddressValue);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.ContactPointNoteId);
        TestContext.Out.WriteLine("Write Contact point Note");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.ContactPointNoteId, DataServiceConstants.Data.ContactNoteValue);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.ContactPointPhoneId);
        TestContext.Out.WriteLine("Write Contact point Phone");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.ContactPointPhoneId, DataServiceConstants.Data.ContactPhoneValue);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.ThemeCodesId);
        TestContext.Out.WriteLine("Write ThemeCodes");
        var arrayThemeCodes = new string[] { DataServiceConstants.EditMask.ThemeCodesValueId1, DataServiceConstants.EditMask.ThemeCodesValueId2, DataServiceConstants.EditMask.ThemeCodesValueId3 };
        await actions.SelectOptionsById(DataServiceConstants.EditMask.ThemeCodesId, arrayThemeCodes);
        var selectedThemes = await actions.ReadMatSelect(DataServiceConstants.EditMask.ThemeCodesId);
        usedVocabularyValues.AddOrUpdate(DataServiceConstants.EditMask.ThemeCodesId, selectedThemes);

        TestContext.Out.WriteLine("Write Keywords");
        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.KeywordsControlName);
        var itemKeywords = new KeyValuePair<string, string>[] { new(DataServiceConstants.EditMask.KeywordsDeId, DataServiceConstants.Data.KeywordsDeValue) };
        await actions.FillEditTableRow(DataServiceConstants.EditMask.KeywordsAddRowId, DataServiceConstants.EditMask.KeywordSaveRowId, 0, itemKeywords);

        TestContext.Out.WriteLine("Write LandingPages");
        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.LandingPagesControlName);
        var itemLandingPages = new KeyValuePair<string, string>[] { new(DataServiceConstants.EditMask.LandingPagesHrefId, DataServiceConstants.Data.LandingPagesHrefValue), new(DataServiceConstants.EditMask.LandingPagesDefId, DataServiceConstants.Data.LandingPagesDeIdValue) };
        await actions.FillEditTableRow(DataServiceConstants.EditMask.LandingPagesAddRowId, DataServiceConstants.EditMask.LandingPagesSaveRowId, 0, itemLandingPages);

        TestContext.Out.WriteLine("Write Conforms To");
        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.ConformsToControlName);
        var itemConformsTos = new KeyValuePair<string, string>[] { new(DataServiceConstants.EditMask.ConformsToHrefId, DataServiceConstants.Data.ConformsToHrefIdValue), new(DataServiceConstants.EditMask.ConformsToDefId, DataServiceConstants.Data.ConformsToDeIdValue) };
        await actions.FillEditTableRow(DataServiceConstants.EditMask.ConformsToAddRowId, DataServiceConstants.EditMask.ConformsToSaveRowId, 0, itemConformsTos);

        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.DocumentsControlName);
        TestContext.Out.WriteLine("Write Documents");
        var itemDocuments = new KeyValuePair<string, string>[] { new(DataServiceConstants.EditMask.DocumentsHrefId, DataServiceConstants.Data.DocumentsHrefIdValue), new(DataServiceConstants.EditMask.DocumentsDefId, DataServiceConstants.Data.DocumentsDefIdValue) };
        await actions.FillEditTableRow(DataServiceConstants.EditMask.DocumentsAddRowId, DataServiceConstants.EditMask.DocumentsSaveRowId, 0, itemDocuments);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.VersionId);
        TestContext.Out.WriteLine("Write Version");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.VersionId, DataServiceConstants.Data.VersionValue);

        await actions.ScrollIntoViewById(DataServiceConstants.EditMask.VersionNoteId);
        TestContext.Out.WriteLine("Write Version Note");
        await actions.FillInputAndEnterById(DataServiceConstants.EditMask.VersionNoteId, DataServiceConstants.Data.VersionNoteValue);

        return usedVocabularyValues.AsReadOnly();
    }
}
