
namespace Bfs.Iop.Admin.Testautomation.Constants;

public static class DataServiceConstants
{
    public static class EditMask
    {
        public static readonly string ButtonEditId = "description-edit";

        public static readonly string TitleDeId = "title-description-de";
        public static readonly string DescriptionDeId = "description-description-de";
        public static readonly string PublisherId = "publisher-description";
        public static readonly string EndpointUrlsControlName = "endpointUrls";
        public static readonly string EndpointUrlsTextId = "endpointUrls-href";// plus index
        public static readonly string EndpointUrlsAddRowId = "endpointUrls-add-row";
        public static readonly string EndpointUrlsSaveRowId = "endpointUrls-save-row";// plus index
        public static readonly string ContactPointEmailId = "contactpoint-emailinternet-0";
        public static readonly string AccessRightsId = "accessrights-description";
        public static readonly string AccessRightsOptionId = "accessrights1";

        public static readonly string EndpointDescriptionControlName = "endpointDescriptions";
        public static readonly string EndpointDescriptionTextId = "endpointDescriptions-href";// plus index
        public static readonly string EndpointDescriptionAddRowId = "endpointDescriptions-add-row";
        public static readonly string EndpointDescriptionSaveRowId = "endpointDescriptions-save-row";// plus index
        public static readonly string ContactPointOrganisationId = "contactpoint-de0";
        public static readonly string ContactPointAddressId = "contactpoint-adrwork-de0";
        public static readonly string ContactPointNoteId = "contactpoint-note-de0";
        public static readonly string ContactPointPhoneId = "contactpoint-telworkvoice-0";

        public static readonly string ThemeCodesId = "themeCodes";
        public static readonly string ThemeCodesValueId1 = "themeCodes121";
        public static readonly string ThemeCodesValueId2 = "themeCodes119";
        public static readonly string ThemeCodesValueId3 = "themeCodes120";

        public static readonly string KeywordsControlName = "keywords";
        public static readonly string KeywordsDeId = "keywords-de";
        public static readonly string KeywordsAddRowId = "keywords-add-row";
        public static readonly string KeywordSaveRowId = "keywords-save-row";

        public static readonly string LandingPagesControlName = "landingPages";
        public static readonly string LandingPagesHrefId = "landingPages-href";
        public static readonly string LandingPagesDefId = "landingPages-de";
        public static readonly string LandingPagesAddRowId = "landingPages-add-row";
        public static readonly string LandingPagesSaveRowId = "landingPages-save-row";

        public static readonly string ConformsToControlName = "conformsTo";
        public static readonly string ConformsToHrefId = "conformsTo-href";
        public static readonly string ConformsToDefId = "conformsTo-de";
        public static readonly string ConformsToAddRowId = "conformsTo-add-row";
        public static readonly string ConformsToSaveRowId = "conformsTo-save-row";

        public static readonly string DocumentsControlName = "documentation";
        public static readonly string DocumentsHrefId = "documentation-href";
        public static readonly string DocumentsDefId = "documentation-de";
        public static readonly string DocumentsAddRowId = "documentation-add-row";
        public static readonly string DocumentsSaveRowId = "documentation-save-row";

        public static readonly string VersionId = "version";
        public static readonly string VersionNoteId = "description-versionnotes-de";

        public static readonly string DescriptionSaveAndCloseId = "description-saveandclose";
        public static readonly string CatalogMenuButtonId = "catalog-menubutton";
        public static readonly string CreateDataServiceButtonId = "create-dataservice";
    }

    public static class ViewMask
    {
        public static readonly string DescriptionTitleId = "description-title";
        public static readonly string DescriptionDescriptionId = "description-description";
        public static readonly string DescriptionPublisherId = "description-publisher";
        public static readonly string DescriptionContactPointId = "description-contactpoint";
        public static readonly string DescriptionThemesId = "description-themes";
        public static readonly string DescriptionLandingPageId = "description-landingpage";
        public static readonly string DescriptionAccessRightsId = "description-accessrights";
        public static readonly string DescriptionDocumentationId = "description-documentation";
        public static readonly string DescriptionVersionId = "description-version";
        public static readonly string DescriptionVersionNotesId = "description-versionnotes";
        public static readonly string DescriptionEndpointUrlsId = "description-endpointUrls";
        public static readonly string DescriptionEndpointDescriptionsId = "description-endpoint-descriptions";
        public static readonly string DescriptionConformsToId = "description-conformtos";
    }

    public static class Data
    {
        public static readonly string TitleDataService = "Playwright_Test_DataService";
        public static readonly string IdentifierDataService = "Playwright_Test_DataService";
        public static readonly string OgdTitleDataService = "Playwright_Test_OGD_DataService";
        public static readonly string OgdIdentifierDataService = "Playwright_Test_OGD_DataService";

        public static readonly string HrefIdValue = "https://ethz.ch";
        public static readonly string PublisherViewDe = "I14Y Test Organisation_de";
        public static readonly string ContactPointEmailValue = "aaa.bbb@bit.admin.ch";

        public static readonly string ContactOrganisationValue = "ProjektFokus GmbH";
        public static readonly string ContactAddressValue = "Kirchstrasse 52\n3097 Liebefeld";
        public static readonly string ContactNoteValue = "Seltsam, im Nebel zu wandern!\nEinsam ist jeder Busch und Stein,\nKein Baum sieht den andern,\nJeder ist allein.\n\nVoll von Freunden war mir die Welt,\nAls noch mein Leben licht war;\nNun, da der Nebel fällt,\nIst keiner mehr sichtbar.";
        public static readonly string ContactPhoneValue = "097 102 14 02";

        public static readonly string KeywordsDeValue = "Playwright Keywords-De";

        public static readonly string LandingPagesHrefValue = "https://www.blick.ch";
        public static readonly string LandingPagesDeIdValue = "Ueber allen Gipfeln ist Ruh',in allen Wipfelnspürest Du kaum einen Hauch";

        public static readonly string ConformsToHrefIdValue = "https://www.watson.ch";
        public static readonly string ConformsToDeIdValue = "In die Ecke, Besen, Besen! seid’s gewesen!";

        public static readonly string DocumentsHrefIdValue = "https://ethz.ch";
        public static readonly string DocumentsDefIdValue = "Molecular Systems Engineering";

        public static readonly string VersionValue = "1.0.0";
        public static readonly string VersionNoteValue = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam";
    }
}