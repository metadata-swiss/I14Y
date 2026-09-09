using Bfs.Iop.Admin.Testautomation.Constants;
using Bfs.Iop.Admin.Testautomation.Enums;
using Bfs.Iop.Test.Abstraction.Extensions;
using Bfs.Iop.Test.Abstraction.Helpers;

namespace Bfs.Iop.Admin.Testautomation.Helpers;

internal static class PublicServiceEditMaskHelper
{
    public static async Task<IReadOnlyDictionary<string, string>> FillPublicServiceMaximal(Wrapper actions, string title)
    {
        var usedVocabularyValues = new Dictionary<string, string>();

        TestContext.Out.WriteLine("Write PublicService title");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceTitleDeId);
        await actions.FillInputById(PublicServiceConstants.EditMask.PublicServiceTitleDeId, title);

        TestContext.Out.WriteLine("Write PublicService description");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceDescriptionId);
        await actions.FillInputById(PublicServiceConstants.EditMask.PublicServiceDescriptionId, PublicServiceConstants.Data.Description);

        TestContext.Out.WriteLine("Write PublicService identifier");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceIdentifierId);
        await actions.FillInputById(PublicServiceConstants.EditMask.PublicServiceIdentifierId, title);

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServicePublisherId);
        TestContext.Out.WriteLine("Write PublicService publisher");
        await actions.SelectFirstAutocompleteById(PublicServiceConstants.EditMask.PublicServicePublisherId, PublicServiceConstants.Data.PublisherViewDe);

        TestContext.Out.WriteLine("Write PublicService Language");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceLanguagesId);
        var arrayLanguage = new string[] { PublicServiceConstants.EditMask.PublicServiceOptionLanguageDeId, PublicServiceConstants.EditMask.PublicServiceOptionLanguageEnId };
        await actions.SelectOptionsById(PublicServiceConstants.EditMask.PublicServiceLanguagesId, arrayLanguage);
        var languages = await actions.ReadMatSelect(PublicServiceConstants.EditMask.PublicServiceLanguagesId);
        usedVocabularyValues.AddOrUpdate(PublicServiceConstants.EditMask.PublicServiceLanguagesId, languages);

        TestContext.Out.WriteLine("Write PublicService Themes");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceThemesId);
        var arrayThemes = new string[] { PublicServiceConstants.EditMask.PublicServiceThematicAreaId + "0", PublicServiceConstants.EditMask.PublicServiceThematicAreaId + "2", PublicServiceConstants.EditMask.PublicServiceThematicAreaId + "3" };
        await actions.SelectOptionsById(PublicServiceConstants.EditMask.PublicServiceThemesId, arrayThemes);
        var themes = await actions.ReadMatSelect(PublicServiceConstants.EditMask.PublicServiceThemesId);
        usedVocabularyValues.AddOrUpdate(PublicServiceConstants.EditMask.PublicServiceThemesId, themes);

        TestContext.Out.WriteLine("Write PublicService Sector");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceDescriptionSectorId);
        var arraySector = new string[] { PublicServiceConstants.EditMask.PublicServiceSectorOptionId + "0", PublicServiceConstants.EditMask.PublicServiceSectorOptionId + "2", PublicServiceConstants.EditMask.PublicServiceSectorOptionId + "3" };
        await actions.SelectOptionsById(PublicServiceConstants.EditMask.PublicServiceDescriptionSectorId, arraySector);
        var sectors = await actions.ReadMatSelect(PublicServiceConstants.EditMask.PublicServiceDescriptionSectorId);
        usedVocabularyValues.AddOrUpdate(PublicServiceConstants.EditMask.PublicServiceDescriptionSectorId, sectors);

        TestContext.Out.WriteLine("Write PublicService Business Events");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceDescriptionBusinessEventsId);
        var arrayBusinessEvents = new string[] { PublicServiceConstants.EditMask.PublicServiceBusinessEventsId + "0", PublicServiceConstants.EditMask.PublicServiceBusinessEventsId + "2", PublicServiceConstants.EditMask.PublicServiceBusinessEventsId + "3" };
        await actions.SelectOptionsById(PublicServiceConstants.EditMask.PublicServiceDescriptionBusinessEventsId, arrayBusinessEvents);
        var businessEvents = await actions.ReadMatSelect(PublicServiceConstants.EditMask.PublicServiceDescriptionBusinessEventsId);
        usedVocabularyValues.AddOrUpdate(PublicServiceConstants.EditMask.PublicServiceDescriptionBusinessEventsId, businessEvents);

        TestContext.Out.WriteLine("Write PublicService Life Events");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.PublicServiceDescriptionLifeEventsId);
        var arrayLifeEvents = new string[] { PublicServiceConstants.EditMask.PublicServiceLifeEventsOptionId + "0", PublicServiceConstants.EditMask.PublicServiceLifeEventsOptionId + "2", PublicServiceConstants.EditMask.PublicServiceLifeEventsOptionId + "3" };
        await actions.SelectOptionsById(PublicServiceConstants.EditMask.PublicServiceDescriptionLifeEventsId, arrayLifeEvents);
        var lifeEvents = await actions.ReadMatSelect(PublicServiceConstants.EditMask.PublicServiceDescriptionLifeEventsId);
        usedVocabularyValues.AddOrUpdate(PublicServiceConstants.EditMask.PublicServiceDescriptionLifeEventsId, lifeEvents);

        TestContext.Out.WriteLine("Write PublicService Keywords");
        await actions.ScrollOnTopByControlName(PublicServiceConstants.EditMask.ControlNameKeywords);
        var itemKeywords = new KeyValuePair<string, string>[] { new(PublicServiceConstants.EditMask.KeywordsDeId, PublicServiceConstants.Data.KeywordsDeValue) };
        await actions.FillEditTableRow(PublicServiceConstants.EditMask.KeywordsAddRowId, PublicServiceConstants.EditMask.KeywordSaveRowId, 0, itemKeywords);

        TestContext.Out.WriteLine("Write PublicService Spatial CH");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.FormatSpatialChId);
        var arraySpatialCh = new string[] { PublicServiceConstants.EditMask.PublicServiceOptionFormatId + "0", PublicServiceConstants.EditMask.PublicServiceOptionFormatId + "1", PublicServiceConstants.EditMask.PublicServiceOptionFormatId + "2" };
        await actions.SelectOptionsById(PublicServiceConstants.EditMask.FormatSpatialChId, arraySpatialCh);
        var spatialCh = await actions.ReadMatSelect(PublicServiceConstants.EditMask.FormatSpatialChId);
        usedVocabularyValues.AddOrUpdate(PublicServiceConstants.EditMask.FormatSpatialChId, spatialCh);

        TestContext.Out.WriteLine("Write PublicService Spatial");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.KeywordFormatSpatialId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.KeywordFormatSpatialId, PublicServiceConstants.Data.SpatialDeValue);

        return usedVocabularyValues.AsReadOnly();
    }

    public async static Task FillPublicServiceLinks(Wrapper actions, string publicServiceToLinkTitle, string datasetToLinkTitle)
    {
        TestContext.Out.WriteLine("Write Public Service Requires");
        await actions.ScrollOnTopById(PublicServiceConstants.EditMask.LinkRequiresAddRowButtonId);
        var itemRequires = new KeyValuePair<string, string>[] { new(PublicServiceConstants.EditMask.LinkRequiresServicesLinkId, publicServiceToLinkTitle) };
        await actions.FillLinkedEditTableRow(PublicServiceConstants.EditMask.LinkRequiresAddRowButtonId, 0, PublicServiceConstants.EditMask.LinkRequiresSaveRowButtonId, itemRequires);

        await actions.WaitForSpinnerToDisappear();
        await actions.WaitForNotificationToDisappear();

        TestContext.Out.WriteLine("Write Public Service LinkWith");
        await actions.ScrollOnTopById(PublicServiceConstants.EditMask.LinkWithAddRowButtonId);
        var itemLinkWith = new KeyValuePair<string, string>[] { new(PublicServiceConstants.EditMask.LinkWithServicesLinkId, publicServiceToLinkTitle) };
        await actions.FillLinkedEditTableRow(PublicServiceConstants.EditMask.LinkWithAddRowButtonId, 0, PublicServiceConstants.EditMask.LinkWithSaveRowButtonId, itemLinkWith);

        await actions.WaitForSpinnerToDisappear();
        await actions.WaitForNotificationToDisappear();

        TestContext.Out.WriteLine("Write Public Service IsDescribedAt");
        await actions.ScrollOnTopById(PublicServiceConstants.EditMask.LinkIsDescribedAddRowButtonId);
        var itemIsDescribed = new KeyValuePair<string, string>[] { new(PublicServiceConstants.EditMask.LinkIsDescribedServicesLinkId, datasetToLinkTitle) };
        await actions.FillLinkedEditTableRow(PublicServiceConstants.EditMask.LinkIsDescribedAddRowButtonId, 0, PublicServiceConstants.EditMask.LinkIsDescribedSaveRowButtonId, itemIsDescribed);

        await actions.WaitForNotificationToDisappear();
    }

    public async static Task RemovePublicServiceLinks(Wrapper actions)
    {
        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollPageDown(Wrapper.ScrollMethod.KeyboardScroll);

        // Wait until edit-table-link-requires has been created in DOOM
        await actions.Wait1500();

        TestContext.Out.WriteLine("Unlink Public Services Requires");

        await actions.ScrollOnTopById(PublicServiceConstants.EditMask.LinkRequiresSelectAllCheckboxId);
        await actions.WaitForSpinnerToDisappear();
        await actions.ClickCheckBoxById(PublicServiceConstants.EditMask.LinkRequiresSelectAllCheckboxId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ClickButtonById(PublicServiceConstants.EditMask.LinkRequiresDeleteSelectedId);

        await actions.WaitForSpinnerToDisappear();
        await actions.WaitForNotificationToDisappear();

        TestContext.Out.WriteLine("Unlink Public Services LinkWith");
        await actions.ScrollOnTopById(PublicServiceConstants.EditMask.LinkWithSelectAllCheckboxId);

        await actions.ClickCheckBoxById(PublicServiceConstants.EditMask.LinkWithSelectAllCheckboxId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ClickButtonById(PublicServiceConstants.EditMask.LinkWithDeleteSelectedId);

        await actions.WaitForSpinnerToDisappear();
        await actions.WaitForNotificationToDisappear();

        TestContext.Out.WriteLine("Unlink Public Services IsDescribed");
        await actions.ScrollOnTopById(PublicServiceConstants.EditMask.LinkIsSelectAllCheckboxId);

        await actions.ClickCheckBoxById(PublicServiceConstants.EditMask.LinkIsSelectAllCheckboxId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ClickButtonById(PublicServiceConstants.EditMask.LinkIsDeleteSelectedId);

        await actions.WaitForSpinnerToDisappear();
        await actions.WaitForNotificationToDisappear();
    }

    public async static Task FillCreateChannelMask(Wrapper actions, string channelTitle, ChannelType channelType)
    {
        await actions.Wait500();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditIdentifierId);
        TestContext.Out.WriteLine($"Write Channel Title for type {channelType}");
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelEditIdentifierId, channelTitle);

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditDescriptionDeId);
        TestContext.Out.WriteLine("Write Channel Description");
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelEditDescriptionDeId, PublicServiceConstants.Data.DescriptionChannel);

        switch (channelType)
        {
            case ChannelType.Post:
                await FillPostChannelMask(actions);
                break;

            case ChannelType.Email:
                await FillEmailChannelMask(actions);
                break;

            case ChannelType.Mobile:
                await FillMobileChannelMask(actions);
                break;

            case ChannelType.Fax:
                await FillFaxChannelMask(actions);
                break;

            case ChannelType.Phone:
                await FillPhoneChannelMask(actions);
                break;

            case ChannelType.Internet:
                await FillInternetChannelMask(actions);
                break;

            default:
                throw new NotSupportedException($"The channel type '{channelType}' is not supported.");
        }

        await actions.WaitForSpinnerToDisappear();
    }

    private async static Task FillPostChannelMask(Wrapper actions)
    {
        TestContext.Out.WriteLine("Choose Channel Post");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditTypeId);
        await actions.SelectOptionById(PublicServiceConstants.EditMask.ChannelEditTypeId, PublicServiceConstants.EditMask.ChannelTypeOptionPostId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelAddressDeId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelAddressDeId, PublicServiceConstants.Data.AddressChannel);

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelOpeningHoursId);
        TestContext.Out.WriteLine("Write Channel OpeningHours");
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelOpeningHoursId, PublicServiceConstants.Data.OpeningHoursChannel);
    }

    private async static Task FillEmailChannelMask(Wrapper actions)
    {
        TestContext.Out.WriteLine("Choose Channel Email");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditTypeId);
        await actions.SelectOptionById(PublicServiceConstants.EditMask.ChannelEditTypeId, PublicServiceConstants.EditMask.ChannelTypeOptionMailId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEmailId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelEmailId, PublicServiceConstants.Data.EmailChannel);
    }

    private async static Task FillMobileChannelMask(Wrapper actions)
    {
        TestContext.Out.WriteLine("Choose Channel Mobile");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditTypeId);
        await actions.SelectOptionById(PublicServiceConstants.EditMask.ChannelEditTypeId, PublicServiceConstants.EditMask.ChannelTypeOptionMobileId);

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelMobileId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelMobileId, PublicServiceConstants.Data.MobileChannel);

        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelOpeningHoursId);
        TestContext.Out.WriteLine("Write Channel OpeningHours");
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelOpeningHoursId, PublicServiceConstants.Data.OpeningHoursChannel);
    }

    private async static Task FillFaxChannelMask(Wrapper actions)
    {
        TestContext.Out.WriteLine("Choose Channel Fax");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditTypeId);
        await actions.SelectOptionById(PublicServiceConstants.EditMask.ChannelEditTypeId, PublicServiceConstants.EditMask.ChannelTypeOptionFaxId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelFaxId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelFaxId, PublicServiceConstants.Data.FaxChannel);
    }

    private async static Task FillInternetChannelMask(Wrapper actions)
    {
        TestContext.Out.WriteLine("Choose Channel Internet");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditTypeId);
        await actions.SelectOptionById(PublicServiceConstants.EditMask.ChannelEditTypeId, PublicServiceConstants.EditMask.ChannelTypeOptionInternetId);

        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelUrlId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelUrlId, PublicServiceConstants.Data.InternetChannel);
    }

    private async static Task FillPhoneChannelMask(Wrapper actions)
    {
        TestContext.Out.WriteLine("Choose Channel Phone");
        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelEditTypeId);
        await actions.SelectOptionById(PublicServiceConstants.EditMask.ChannelEditTypeId, PublicServiceConstants.EditMask.ChannelTypeOptionPhoneId);

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelPhoneId);
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelPhoneId, PublicServiceConstants.Data.PhoneChannel);

        await actions.WaitForSpinnerToDisappear();

        await actions.ScrollIntoViewById(PublicServiceConstants.EditMask.ChannelOpeningHoursId);
        TestContext.Out.WriteLine("Write Channel OpeningHours");
        await actions.FillInputAndEnterById(PublicServiceConstants.EditMask.ChannelOpeningHoursId, PublicServiceConstants.Data.OpeningHoursChannel);
    }
}
