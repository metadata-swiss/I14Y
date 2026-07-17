namespace Bfs.Iop.Admin.Testautomation.Constants;

internal static class PublicServiceConstants
{
    public static class EditMask
    {
        public static readonly string PublicServiceTitleFrId = "title-description-fr";
        public static readonly string PublicServiceTitleDeId = "title-description-de";
        public static readonly string PublicServiceDescriptionId = "description-description-de";
        public static readonly string PublicServiceIdentifierId = "identifiers-description-0";
        public static readonly string PublicServicePublisherId = "description-publisher";
        public static readonly string PublicServiceLanguagesId = "description-languages";
        public static readonly string PublicServiceOptionLanguageDeId = "select-option-de";
        public static readonly string PublicServiceOptionLanguageEnId = "select-option-en";
        public static readonly string PublicServiceThemesId = "description-themes";
        public static readonly string PublicServiceThematicAreaId = "thematic-area";// plus index
        public static readonly string PublicServiceDescriptionSectorId = "description-sector";
        public static readonly string PublicServiceSectorOptionId = "sector"; // plus index
        public static readonly string PublicServiceDescriptionLifeEventsId = "description-life-events";
        public static readonly string PublicServiceLifeEventsOptionId = "life-event";// plus index
        public static readonly string PublicServiceDescriptionBusinessEventsId = "description-business-events";
        public static readonly string PublicServiceBusinessEventsId = "business-event";// plus index
        public static readonly string ControlNameKeywords = "keywords";
        public static readonly string KeywordsDeId = "keywords-de"; // plus index
        public static readonly string KeywordsAddRowId = "keywords-add-row";
        public static readonly string KeywordSaveRowId = "keywords-save-row";
        public static readonly string FormatSpatialChId = "distribution-format-spatial-ch";
        public static readonly string PublicServiceOptionFormatId = "option-format";// plus index
        public static readonly string KeywordFormatSpatialId = "distribution-format-spatial";
        public static readonly string DescriptionSaveAndCloseId = "description-saveandclose";

        public static readonly string LinkRequiresAddRowButtonId = "link-requires-addrow-button";
        public static readonly string LinkRequiresServicesLinkId = "public-services-link-";// plus index
        public static readonly string LinkRequiresSaveRowButtonId = "link-requires-save-button-";// plus index
        public static readonly string LinkRequiresSelectAllCheckboxId = "link-requires-checkbox-all";
        public static readonly string LinkRequiresDeleteSelectedId = "link-requires-deleteSelected-button";

        public static readonly string LinkWithAddRowButtonId = "linked-with-addrow-button";
        public static readonly string LinkWithServicesLinkId = "linked-with-link-";// plus index
        public static readonly string LinkWithSaveRowButtonId = "linked-with-save-button-";// plus index
        public static readonly string LinkWithSelectAllCheckboxId = "linked-with-checkbox-all-input";
        public static readonly string LinkWithDeleteSelectedId = "linked-with-deleteSelected-button";

        public static readonly string LinkIsDescribedAddRowButtonId = "link-is-described-addrow-button";
        public static readonly string LinkIsDescribedServicesLinkId = "link-is-described-title-";// plus index
        public static readonly string LinkIsDescribedSaveRowButtonId = "link-is-described-save-button-";// plus index
        public static readonly string LinkIsSelectAllCheckboxId = "link-is-described-checkbox-all-input";
        public static readonly string LinkIsDeleteSelectedId = "link-is-described-delete-selected-button";

        public static readonly string ChannelTableId = "channel-table";
        public static readonly string ChannelTableAddRowButtonId = "channel-add-row";
        public static readonly string ChannelSaveAndCloseId2 = "edit-channel-apply-button";
        public static readonly string ChannelEditIdentifierId = "channel-identifier";
        public static readonly string ChannelEditDescriptionDeId = "description-channel-de";
        public static readonly string ChannelEditTypeId = "channel-type";
        public static readonly string ChannelTypeOptionPostId = "channel-type-0c84394663";
        public static readonly string ChannelTypeOptionMailId = "channel-type-1fc1caefa8";
        public static readonly string ChannelTypeOptionMobileId = "channel-type-5c12931e3f";
        public static readonly string ChannelTypeOptionFaxId = "channel-type-a1c5444664";
        public static readonly string ChannelTypeOptionInternetId = "channel-type-b37115f83e";
        public static readonly string ChannelTypeOptionPhoneId = "channel-type-c05134f579";
        public static readonly string ChannelAddressDeId = "channel-address-de";
        public static readonly string ChannelOpeningHoursId = "channel-openinghours";
        public static readonly string ChannelEmailId = "channel-email";
        public static readonly string ChannelMobileId = "channel-mobile";
        public static readonly string ChannelFaxId = "channel-fax";
        public static readonly string ChannelUrlId = "channel-url";
        public static readonly string ChannelPhoneId = "channel-phone";
        public static readonly string ChannelTableDeleteAllButtonId = "channel-deleteSelected-row";
        public static readonly string ChannelTableConfirmDeleteDialogButtonId = "confirm-dialog";
    }

    public static class ViewMask
    {
        public static readonly string CreatePublicServiceButtonId = "create-publicservice";
        public static readonly string CatalogSearchId = "catalog-search";
        public static readonly string CatalogTableId = "catalog-table";

        public static readonly string PublicServiceViewTitleId = "description-title";
        public static readonly string PublicServiceViewDescriptionId = "description-description";
        public static readonly string PublicServiceViewIdentifierId = "description-identifier";
        public static readonly string PublicServiceViewPublisherId = "description-publisher";
        public static readonly string PublicServiceViewThemesId = "description-themes";
        public static readonly string PublicServiceViewKeywordsId = "description-keywords";
        public static readonly string PublicServiceViewSectorId = "description-sector";
        public static readonly string PublicServiceViewLanguagesId = "description-languages";
        public static readonly string PublicServiceViewBusinessEventsId = "decription-business-events";
        public static readonly string PublicServiceViewLifeEventsId = "description-life-events";
        public static readonly string PublicServiceViewSpatialChId = "description-spatial-ch";
        public static readonly string PublicServiceViewSpatialId = "description-spatial";

        public static readonly string PublicServiceViewRequiresId = "description-requires";
        public static readonly string PublicServiceRequiresId = "require-";// plus index
        public static readonly string PublicServiceViewRelationId = "description-relation";
        public static readonly string PublicServiceRelationId = "relation-";// plus index
        public static readonly string PublicServiceViewDescribedId = "description-described";
        public static readonly string PublicServiceDescribedId = "described-";// plus index

        public static readonly string ChannelTableIdentifierId = "detail-identifier-"; // plus index
        public static readonly string ChannelTableTypeId = "detail-type-"; // plus index
        public static readonly string ChannelEditCreateId = "channel-create-button";

        public static readonly string CatalogTableViewButton = "view-detail-button-";

        public static readonly string PublicServiceViewDeleteButtonId = "public-service-view-delete-button";

        public static readonly string PublicServiceTabDescriptionId = "publicservice-tabs-description";
        public static readonly string PublicServiceTabChannelId = "publicservice-tabs-channel";
        public static readonly string PublicServiceViewEditButtonId = "description-edit";
    }

    public static class Data
    {
        public static readonly string TitlePublicServices = "Playwright_Test_PublicServices";
        public static readonly string Description = "Playwright_Test : Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam";
        public static readonly string KeywordsDeValue = "Playwright Keywords-De";
        public static readonly string SpatialDeValue = "Playwright Spatial-De";
        public static readonly string PublisherViewDe = "I14Y Test Organisation_de";

        public static readonly string DescriptionChannel = "Playwright_Test : Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam";
        public static readonly string AddressChannel = @"Krichstrasse 52
3097 Liebefeld
Swiss";
        public static readonly string OpeningHoursChannel = "Mo-Fr 08:00-12:00, 13:00-17:30, Sa 08:00-12:00";
        public static readonly string EmailChannel = "aabb.toto@example.ch";
        public static readonly string MobileChannel = "0791021402";
        public static readonly string FaxChannel = "+00 12 345 56 76";
        public static readonly string InternetChannel = "https://www.i14y.ch";
        public static readonly string PhoneChannel = "0313715381";

        public static readonly string ChannelViewTypePostDE = "Adresse";
        public static readonly string ChannelViewTypeEmailDE = "E-Mail-Adresse";
        public static readonly string ChannelViewTypeMobileDE = "Mobil";
        public static readonly string ChannelViewTypeFaxDE = "Fax";
        public static readonly string ChannelViewTypeInternetDE = "Internet";
        public static readonly string ChannelViewTypePhoneDE = "Telefon";

        public static readonly string NoneView = "-";
    }
}
