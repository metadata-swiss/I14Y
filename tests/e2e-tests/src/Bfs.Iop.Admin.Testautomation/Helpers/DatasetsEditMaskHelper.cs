using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Test.Abstraction.Extensions;
using Bfs.Iop.Test.Abstraction.Helpers;

namespace Bfs.Iop.Admin.Testautomation.Helpers;

internal static class DatasetsEditMaskHelper
{
    public static async Task FillDatasetMinimal(
        Wrapper actions,
        string title,
        string identifier,
        string currentDate)
    {
        await actions.ScrollIntoViewById(DatasetConstants.EditMask.TitleDeId);
        TestContext.Out.WriteLine($"Write Title: = {title}");
        await actions.FillInputById(DatasetConstants.EditMask.TitleDeId, title);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DescriptionDeId);
        TestContext.Out.WriteLine("Write Description");
        await actions.FillInputById(DatasetConstants.EditMask.DescriptionDeId, DatasetConstants.Data.Description);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.IdentifierId);
        TestContext.Out.WriteLine("Write Identifier");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.IdentifierId, identifier);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.PublisherId);
        TestContext.Out.WriteLine("Write Publisher");
        await actions.SelectFirstAutocompleteById(DatasetConstants.EditMask.PublisherId, DatasetConstants.Data.PublisherViewDe);
        
        await actions.ScrollIntoViewById(DatasetConstants.EditMask.AccessRightsId);
        TestContext.Out.WriteLine("Write AccessRights");
        await actions.SelectOptionById(DatasetConstants.EditMask.AccessRightsId, DatasetConstants.EditMask.AccessRightsOptionId);
        
        TestContext.Out.WriteLine("Write DataOwner");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DataOwnerId, DatasetConstants.Data.DataOwnerName);
        
        TestContext.Out.WriteLine("Write ResponsiblePerson");
        await actions.SelectFirstAutocompleteById(DatasetConstants.EditMask.ResponsiblePersonId, DatasetConstants.Data.ResponsiblePersonName);
        
        await actions.ScrollIntoViewById(DatasetConstants.EditMask.PublicationDateId);
        TestContext.Out.WriteLine("Write PublicationDate");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.PublicationDateId, currentDate);
        
        await actions.ScrollIntoViewById(DatasetConstants.EditMask.ContactPointEmailId + "0");
        TestContext.Out.WriteLine("Write Contact point email");
        await actions.FillInputById(DatasetConstants.EditMask.ContactPointEmailId + "0", DatasetConstants.Data.ContactPointEmailValue);
    }

    public async static Task<IReadOnlyDictionary<string, string>> FillDatasetMaximal(
        Wrapper actions,
        string title,
        string identifier,
        string currentDate,
        string periodDate)
    {
        await FillDatasetMinimal(actions, title, identifier, currentDate);

        return await CompleteDatasetMaximal(actions, currentDate, periodDate);
    }

    public async static Task<IReadOnlyDictionary<string, string>> FillToUpdateDatasetMaximal(
        Wrapper actions,
        string currentDate,
        string periodDate)
    {
        // This method must only be called after existing a dataset with minimal information

        await actions.Wait1000();

        return await CompleteDatasetMaximal(actions, currentDate, periodDate);
    }

    private static async Task<IReadOnlyDictionary<string, string>> CompleteDatasetMaximal(
        Wrapper actions,
        string currentDate,
        string periodDate)
    {
        var usedVocabularyValues = new Dictionary<string, string>();

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.AccessRightsId);
        TestContext.Out.WriteLine("Write AccessRights");
        await actions.ChooseFirstOptionById(DatasetConstants.EditMask.AccessRightsId);
        var accessRightsValue = await actions.ReadMatSelect(DatasetConstants.EditMask.AccessRightsId);
        usedVocabularyValues.AddOrUpdate(DatasetConstants.EditMask.AccessRightsId, accessRightsValue);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.PublicationDateId);
        TestContext.Out.WriteLine("Write PublicationDate");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.PublicationDateId, currentDate);

        await FillContactPoint(actions);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectLanguageId);
        TestContext.Out.WriteLine("Write Language");
        var arrayLanguage = new string[] { DatasetConstants.EditMask.OptionLanguageId + "0", DatasetConstants.EditMask.OptionLanguageId + "2", DatasetConstants.EditMask.OptionLanguageId + "3" };
        await actions.SelectOptionsById(DatasetConstants.EditMask.SelectLanguageId, arrayLanguage);
        var languages = await actions.ReadMatSelect(DatasetConstants.EditMask.SelectLanguageId);
        usedVocabularyValues.AddOrUpdate(DatasetConstants.EditMask.SelectLanguageId, languages);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectThemeCodesId);
        TestContext.Out.WriteLine("Write ThemeCodes");
        var arrayThemeCodes = new string[] { DatasetConstants.EditMask.ThemeCodesValueId1, DatasetConstants.EditMask.ThemeCodesValueId2, DatasetConstants.EditMask.ThemeCodesValueId3 };
        await actions.SelectOptionsById(DatasetConstants.EditMask.SelectThemeCodesId, arrayThemeCodes);
        var selectedThemes = await actions.ReadMatSelect(DatasetConstants.EditMask.SelectThemeCodesId);
        usedVocabularyValues.AddOrUpdate(DatasetConstants.EditMask.SelectThemeCodesId, selectedThemes);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.RetentionPeriodId);
        TestContext.Out.WriteLine("Write Retention Period");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.RetentionPeriodId, periodDate);

        TestContext.Out.WriteLine("Write Keywords");
        await actions.ScrollOnTopByControlName(DataServiceConstants.EditMask.KeywordsControlName);
        var itemKeywords = new KeyValuePair<string, string>[] { new(DatasetConstants.EditMask.KeywordsDeId, DatasetConstants.Data.KeywordsDeValue) };
        await actions.FillEditTableRow(DatasetConstants.EditMask.KeywordsAddRowId, DatasetConstants.EditMask.KeywordSaveRowId, 0, itemKeywords);

        TestContext.Out.WriteLine("Write LandingPages");
        var itemLandingPages = new KeyValuePair<string, string>[] { new(DatasetConstants.EditMask.LandingPagesHrefId, DatasetConstants.Data.LandingPagesHrefValue), new(DatasetConstants.EditMask.LandingPagesDefId, DatasetConstants.Data.LandingPagesDeIdValue) };
        await actions.FillEditTableRow(DatasetConstants.EditMask.LandingPagesAddRowId, DatasetConstants.EditMask.LandingPagesSaveRowId, 0, itemLandingPages);

        TestContext.Out.WriteLine("Write Spatial");
        await actions.ScrollIntoViewById(DatasetConstants.EditMask.SpatialId);
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.SpatialId, DatasetConstants.Data.SpatialIdValue);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectGeoIvId);
        TestContext.Out.WriteLine("Write SelectGeoIv");
        await actions.SelectOptionsById(DatasetConstants.EditMask.SelectGeoIvId, [DatasetConstants.EditMask.OptionGeoIv4Id]);
        var selectedGeoIvValue = await actions.ReadMatSelect(DatasetConstants.EditMask.SelectGeoIvId);
        usedVocabularyValues.AddOrUpdate(DatasetConstants.EditMask.SelectGeoIvId, selectedGeoIvValue);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.CoverageFromId);
        TestContext.Out.WriteLine("Write Temporal coverage");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.CoverageFromId, currentDate);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.SelectFrequencyId);
        TestContext.Out.WriteLine("Write Frequency");
        await actions.ChooseFirstOptionById(DatasetConstants.EditMask.SelectFrequencyId);
        var selectedFrequencyValue = await actions.ReadMatSelect(DatasetConstants.EditMask.SelectFrequencyId);
        usedVocabularyValues.AddOrUpdate(DatasetConstants.EditMask.SelectFrequencyId, selectedFrequencyValue);

        TestContext.Out.WriteLine("Write ConformsTos");
        await actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameConformsTo);
        var itemConformsTos = new KeyValuePair<string, string>[] { new(DatasetConstants.EditMask.ConformsToHrefId, DatasetConstants.Data.ConformsToHrefIdValue), new(DatasetConstants.EditMask.ConformsToDefId, DatasetConstants.Data.ConformsToDeIdValue) };
        await actions.FillEditTableRow(DatasetConstants.EditMask.ConformsToAddRowId, DatasetConstants.EditMask.ConformsToSaveRowId, 0, itemConformsTos);

        TestContext.Out.WriteLine("Write IsReferenced");
        await actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameIsReferencedBy);
        var itemIsReferenced = new KeyValuePair<string, string>[] { new(DatasetConstants.EditMask.IsReferencedByHrefId, DatasetConstants.Data.IsReferencedByHrefIdValue), new(DatasetConstants.EditMask.IsReferencedByDefId, DatasetConstants.Data.IsReferencedByDeIdValue) };
        await actions.FillEditTableRow(DatasetConstants.EditMask.IsReferencedByAddRowId, DatasetConstants.EditMask.IsReferencedBySaveRowId, 0, itemIsReferenced);

        return usedVocabularyValues.AsReadOnly();
    }

    public static async Task FillCreateDistributionMinimal(
        Wrapper actions, 
        string title, 
        string identifier, 
        string currentDate)
    {
        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionTitleId);
        TestContext.Out.WriteLine("Write Distribution Title");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionTitleId, title);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionDescriptionId);
        TestContext.Out.WriteLine("Write Distribution Description");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionDescriptionId, identifier);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionIdentifierId);
        TestContext.Out.WriteLine("Write Distribution Identifier");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionIdentifierId, identifier);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionDateId);
        TestContext.Out.WriteLine("Write Distribution Date");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionDateId, currentDate);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionAccessUrlId);
        TestContext.Out.WriteLine("Write Distribution Access Url");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionAccessUrlId, DatasetConstants.Data.AccessUrlDistributionIdValue);
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionAccessUrlTitleDeId, DatasetConstants.Data.AccessUrlTitleDeDistributionIdValue);
        await actions.ClickCheckBoxById(DatasetConstants.EditMask.DistributionAccessUrlDownloadId);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionLicenseId);
        TestContext.Out.WriteLine("Write Distribution License");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionLicenseId, DatasetConstants.EditMask.DistributionOptionLicenseId + "0");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="title"></param>
    /// <param name="identifier"></param>
    /// <param name="currentDate"></param>
    /// <returns>Returns a dictionary containing the vocabularies values used.</returns>
    public static async Task<IReadOnlyDictionary<string, string>> FillCreateDistributionMaximal(
        Wrapper actions,
        string title,
        string identifier,
        string currentDate)
    {
        var usedVocabularyValues = new Dictionary<string, string>();

        TestContext.Out.WriteLine("fill the mask distribution for the generated dataset");

        await actions.Wait1000();

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionTitleId);
        TestContext.Out.WriteLine("Write Distribution Title");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionTitleId, title);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionDescriptionId);
        TestContext.Out.WriteLine("Write Distribution Description");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionDescriptionId, identifier);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionIdentifierId);
        TestContext.Out.WriteLine("Write Distribution Identifier");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionIdentifierId, identifier);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionDateId);
        TestContext.Out.WriteLine("Write Distribution Date");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionDateId, currentDate);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionLanguageId);
        TestContext.Out.WriteLine("Write Distribution Language");
        var arrayLanguage = new string[] { DatasetConstants.EditMask.DistributionOptionLanguageId + "0", DatasetConstants.EditMask.DistributionOptionLanguageId + "2", DatasetConstants.EditMask.DistributionOptionLanguageId + "3" };
        await actions.SelectOptionsById(DatasetConstants.EditMask.DistributionLanguageId, arrayLanguage);
        var languages = await actions.ReadMatSelect(DatasetConstants.EditMask.DistributionLanguageId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionLanguageId, languages);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionAccessUrlId);
        TestContext.Out.WriteLine("Write Distribution Access Url");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionAccessUrlId, DatasetConstants.Data.AccessUrlDistributionIdValue);
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionAccessUrlTitleDeId, DatasetConstants.Data.AccessUrlTitleDeDistributionIdValue);
        await actions.ClickCheckBoxById(DatasetConstants.EditMask.DistributionAccessUrlDownloadId);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionFilesSizeId);
        TestContext.Out.WriteLine("Write Distribution FilesSize");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionFilesSizeId, "1024");

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionFormatId);
        TestContext.Out.WriteLine("Write Distribution Format");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionFormatId, DatasetConstants.EditMask.DistributionOptionFormatId + "0");

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        var valueFormat = await actions.ReadInput(DatasetConstants.EditMask.DistributionFormatId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionFormatId, valueFormat);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionMediaTypeId);
        TestContext.Out.WriteLine("Write Distribution Media type");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionMediaTypeId, DatasetConstants.EditMask.DistributionOptionMediaTypeId + "0");

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        var valueMediaType = await actions.ReadInput(DatasetConstants.EditMask.DistributionMediaTypeId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionMediaTypeId, valueMediaType);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionPackagingFormatId);
        TestContext.Out.WriteLine("Write Distribution Packaging format");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionPackagingFormatId, DatasetConstants.EditMask.DistributionOptionPackagingFormatId + "1");

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        var valuePackagingFormat = await actions.ReadInput(DatasetConstants.EditMask.DistributionPackagingFormatId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionPackagingFormatId, valuePackagingFormat);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionAlgorithmId);
        TestContext.Out.WriteLine("Write Distribution Algorithm");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionAlgorithmId, DatasetConstants.EditMask.DistributionOptionAlgorithmId + "1");
        var algorithmValue = await actions.ReadInput(DatasetConstants.EditMask.DistributionAlgorithmId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionAlgorithmId, algorithmValue);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionChecksumId);
        TestContext.Out.WriteLine("Write Distribution Checksum Value");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionChecksumId, DatasetConstants.Data.ChecksumViewDe);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionLicenseId);
        TestContext.Out.WriteLine("Write Distribution License");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionLicenseId, DatasetConstants.EditMask.DistributionOptionLicenseId + "0");

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        var valueLicense = await actions.ReadInput(DatasetConstants.EditMask.DistributionLicenseId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionLicenseId, valueLicense);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionRightsId);
        TestContext.Out.WriteLine("Write Distribution Rights");
        await actions.FillInputById(DatasetConstants.EditMask.DistributionRightsId, DatasetConstants.Data.RightsDistributionIdValue);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionAvailabilityId);
        TestContext.Out.WriteLine("Write Distribution Availability");
        await actions.SelectOptionById(DatasetConstants.EditMask.DistributionAvailabilityId, DatasetConstants.EditMask.DistributionOptionAvailabilityId + "1");

        // Since the vocabularies differ depending on the environment, the content must be checked using a dictionary.
        var valueAvailability = await actions.ReadInput(DatasetConstants.EditMask.DistributionAvailabilityId);
        usedVocabularyValues.Add(DatasetConstants.EditMask.DistributionAvailabilityId, valueAvailability);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionCoverageFromId);
        TestContext.Out.WriteLine("Write Distribution Coverage From");
        await actions.FillInputById(DatasetConstants.EditMask.DistributionCoverageFromId, currentDate);

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.DistributionTemporalResolutionId);
        TestContext.Out.WriteLine("Write Distribution Temporal Resolution");
        await actions.FillInputAndEnterById(DatasetConstants.EditMask.DistributionTemporalResolutionId, DatasetConstants.Data.TemporalResolutionDistributionIdValue);

        await actions.ScrollOnTopByControlName(DatasetConstants.EditMask.ControlNameConformsTo);
        TestContext.Out.WriteLine("Write Distribution ConformsTos");
        var itemConformsTos = new KeyValuePair<string, string>[] { new(DatasetConstants.EditMask.DistributionConformsToHrefId, DatasetConstants.Data.ConformsToHrefIdValue), new(DatasetConstants.EditMask.DistributionConformsToValueDeId, DatasetConstants.Data.ConformsToDeIdValue) };
        await actions.FillEditTableRow(DatasetConstants.EditMask.DistributionConformsToAddRowId, DatasetConstants.EditMask.DistributionConformsToSaveId, 0, itemConformsTos);

        await actions.ScrollOnTopByControlName(DatasetConstants.EditMask.DistributionDocumentsControlName);
        TestContext.Out.WriteLine("Write Documents");
        var itemDocuments = new KeyValuePair<string, string>[] 
        {
            new(DatasetConstants.EditMask.DistributionDocumentsUrlTextboxId, DatasetConstants.Data.DocumentsHrefIdValue),
            new(DatasetConstants.EditMask.DistributionDocumentsTitleDeTextboxId, DatasetConstants.Data.DocumentsDefIdValue)
        };
        await actions.FillEditTableRow(DatasetConstants.EditMask.DistributionDocumentsNewRowButtonId, DatasetConstants.EditMask.DistributionDocumentsSaveButtonId, 0, itemDocuments);

        await actions.ScrollOnTopByControlName(DatasetConstants.EditMask.DistributionImagesControlName);
        TestContext.Out.WriteLine("Write Image");
        var itemImage = new KeyValuePair<string, string>[]
        { 
            new(DatasetConstants.EditMask.DistributionImagesUrlTextboxId, DatasetConstants.Data.ImageHrefIdValue), 
            new(DatasetConstants.EditMask.DistributionImagesTitleDeTextboxId, DatasetConstants.Data.ImageDefIdValue) 
        };
        await actions.FillEditTableRow(DatasetConstants.EditMask.DistributionImagesNewRowButtonId, DatasetConstants.EditMask.DistributionImagesSaveButtonId, 0, itemImage);

        return usedVocabularyValues;
    }

    private static async Task FillContactPoint(Wrapper actions)
    {
        TestContext.Out.WriteLine("Write Contact Point");

        await actions.ScrollIntoViewById(DatasetConstants.EditMask.ContactPointId + "0");
        await actions.FillInputById(DatasetConstants.EditMask.ContactPointId + "0", DatasetConstants.Data.ContactPointValue);

        await actions.FillInputById(DatasetConstants.EditMask.ContactAddressId + "0", DatasetConstants.Data.ContactAddressValue);

        await actions.FillInputById(DatasetConstants.EditMask.ContactNoteId + "0", DatasetConstants.Data.ContactNoteValue);

        await actions.FillInputById(DatasetConstants.EditMask.ContactPointEmailId + "0", DatasetConstants.Data.ContactPointEmailValue);
    }
}
