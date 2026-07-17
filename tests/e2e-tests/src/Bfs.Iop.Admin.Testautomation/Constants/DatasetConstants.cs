namespace Bfs.Iop.Admin.Testautomation.Constants;

public static class DatasetConstants
{
    public static class EditMask
    {
        #region Navigation
        public static readonly string CatalogMenuButtonId = "catalog-menubutton";
        public static readonly string CreateDatasetButtonId = "create-dataset";
        public static readonly string CatalogSearchId = "catalog-search";
        public static readonly string CatalogTableId = "catalog-table";
        public static readonly string CatalogTableViewButton = "view-detail-button-";
        public static readonly string ButtonCatalogLinkId = "catalogue-link";
        public static readonly string MenuButtonCatalogLinkId = "catalog-button";

        #endregion

        #region Tabs
        public static readonly string TabDescriptionId = "datasets-tabs-description";
        public static readonly string TabDistributionId = "datasets-tabs-distributions";
        #endregion

        #region Description
        public static readonly string TitleDeId = "title-description-de";
        public static readonly string DescriptionDeId = "description-description-de";
        public static readonly string IdentifierId = "identifiers-description-0";
        public static readonly string PublisherId = "description-publisher";
        public static readonly string AccessRightsId = "accessrights-description";
        public static readonly string PublicationDateId = "publicationdate-description";
        public static readonly string ModifiedDateId = "modifieddate-description";
        public static readonly string ButtonEditId = "description-edit";
        public static readonly string SelectLanguageId = "description-languages";
        public static readonly string OptionLanguageId = "languages"; // + index
        public static readonly string AccessRightsOptionId = "PUBLIC";
        public static readonly string DescriptionSaveAndCloseId = "description-saveandclose";
        public static readonly string ContactPointEmailId = "contactpoint-emailinternet"; // plus index
        #endregion

        #region Contact
        public static readonly string ContactPointId = "contactpoint-de";
        public static readonly string ContactAddressId = "contactpoint-adrwork-de";
        public static readonly string ContactNoteId = "contactpoint-note-de";
        public static readonly string DataOwnerId = "description-dataOwner";
        public static readonly string ResponsiblePersonId = "description-responsiblePerson";
        #endregion

        #region Themes and Keywords
        public static readonly string SelectThemeCodesId = "description-themeCodes";
        public static readonly string ThemeCodesValueId1 = "themeCodes121";
        public static readonly string ThemeCodesValueId2 = "themeCodes119";
        public static readonly string ThemeCodesValueId3 = "themeCodes120";
        public static readonly string KeywordsAddRowId = "keywords-add-row";
        public static readonly string KeywordsDeId = "keywords-de";
        public static readonly string KeywordSaveRowId = "keywords-save-row";
        public static readonly string SelectCatalogThemesId = "catalog-themes";
        #endregion

        #region External References
        public static readonly string LandingPagesAddRowId = "landingPages-add-row";
        public static readonly string LandingPagesHrefId = "landingPages-href";
        public static readonly string LandingPagesDefId = "landingPages-de";
        public static readonly string LandingPagesSaveRowId = "landingPages-save-row";
        public static readonly string ConformsToAddRowId = "conformsTo-add-row";
        public static readonly string ConformsToHrefId = "conformsTo-href";
        public static readonly string ConformsToDefId = "conformsTo-de";
        public static readonly string ConformsToSaveRowId = "conformsTo-save-row";
        public static readonly string IsReferencedByAddRowId = "isReferencedBy-add-row";
        public static readonly string IsReferencedByHrefId = "isReferencedBy-href";
        public static readonly string IsReferencedByDefId = "isReferencedBy-de";
        public static readonly string IsReferencedBySaveRowId = "isReferencedBy-save-row";
        #endregion

        #region controlName
        public static readonly string ControlNameConformsTo = "conformsTo";
        public static readonly string ControlNameKeywords = "keywords";
        public static readonly string ControlNameLandingPages = "landingPages";
        public static readonly string ControlNameIsReferencedBy = "isReferencedBy";
        public static readonly string ControlNameQualifiedRelation = "qualifiedRelations";
        #endregion controlName

        #region Distribution Base
        public static readonly string DistributionButtonId = "distribution-create";
        public static readonly string DistributionTitleId = "title-distribution-de";
        public static readonly string DistributionDescriptionId = "distribution-description-de";
        public static readonly string DistributionIdentifierId = "publication-identifier";
        public static readonly string DistributionDateId = "publication-date-distribution";
        public static readonly string DistributionLanguageId = "languages-distribution";
        public static readonly string DistributionOptionLanguageId = "option-language";
        public static readonly string DistributionSaveAndCloseId = "distribution-saveandclose";
        public static readonly string DistributionTableId = "distribution-table";
        public static readonly string DistributionEditButtonRowId = "edit-detail-button-";
        public static readonly string DistributionViewButtonRowId = "view-detail-button-";
        public static readonly string DistributionDocumentsControlName = "documentation";
        public static readonly string DistributionDocumentsNewRowButtonId = "documentation-add-row";
        public static readonly string DistributionDocumentsSaveButtonId = "documentation-save-row"; // + index
        public static readonly string DistributionImagesControlName = "images";
        public static readonly string DistributionImagesNewRowButtonId = "images-add-row";
        public static readonly string DistributionImagesSaveButtonId = "images-save-row"; // + index
        public static readonly string DistributionConformsToAddRowId = "conformsTo-add-row";
        public static readonly string DistributionConformsToSaveId = "conformsTo-save-row";
        public static readonly string DistributionConformsToValueDeId = "conformsTo-de";
        public static readonly string DistributionConformsToHrefId = "conformsTo-href";
        public static readonly string DistributionAccessUrlId = "access-url-distribution";
        public static readonly string DistributionAccessUrlTitleDeId = "access-url-title-de-distribution";
        public static readonly string DistributionAccessUrlDownloadId = "access-url-download-distribution";
        public static readonly string DistributionFilesSizeId = "filesize-distribution";
        public static readonly string DistributionFormatId = "format-distribution";
        public static readonly string DistributionOptionFormatId = "option-format";
        public static readonly string DistributionMediaTypeId = "mediatype-distribution";
        public static readonly string DistributionOptionMediaTypeId = "option-mediatype";
        public static readonly string DistributionPackagingFormatId = "packagingformat-distribution";
        public static readonly string DistributionOptionPackagingFormatId = "option-packagingformat";
        public static readonly string DistributionAlgorithmId = "algorithm-distribution";
        public static readonly string DistributionOptionAlgorithmId = "option-algorithm";
        public static readonly string DistributionChecksumId = "checksum-value-distribution";
        public static readonly string DistributionLicenseId = "license-distribution";
        public static readonly string DistributionOptionLicenseId = "option-license";
        public static readonly string DistributionRightsId = "rights-distribution";
        public static readonly string DistributionCoverageFromId = "coverage-from-distribution";
        public static readonly string DistributionCoverageToId = "coverage-to-distribution";
        public static readonly string DistributionAvailabilityId = "availability-distribution";
        public static readonly string DistributionOptionAvailabilityId = "option-availability";
        public static readonly string DistributionTemporalResolutionId = "temporalresolution-distribution";
        public static readonly string DistributionDocumentsUrlTextboxId = "documentation-href"; // + index
        public static readonly string DistributionDocumentsTitleDeTextboxId = "documentation-de"; // + index
        public static readonly string DistributionImagesUrlTextboxId = "images-href"; // + index
        public static readonly string DistributionImagesTitleDeTextboxId = "images-de"; // + index
        #endregion

        #region Distribution Service
        public static readonly string DistributionDataServiceLinkAddRowId = "dataservice-add-row";
        public static readonly string DistributionDataServiceOptionTitleId = "dataservice-option-title";
        public static readonly string DistributionDataServiceSaveRowId = "dataservice-save-row";
        public static readonly string DistributionDataServiceSelectAllRowsId = "dataservice-select-all-rows";
        public static readonly string DistributionDataServiceRemoveAllRowsId = "dataservice-remove-rows";
        #endregion

        #region Metadata and Additional Properties
        public static readonly string RetentionPeriodId = "description-retention-period";
        public static readonly string SpatialId = "input-spatial-0";
        public static readonly string SelectGeoIvId = "select-geoiv";
        //public static readonly string OptionGeoIv1Id = "option-geoiv31";
        public static readonly string OptionGeoIv2Id = "option-geoiv179";
        //public static readonly string OptionGeoIv3Id = "option-geoiv228.8";
        public static readonly string OptionGeoIv4Id = "option-geoiv42.17";
        public static readonly string CoverageFromId = "input-coverage-from-0";
        public static readonly string SelectFrequencyId = "select-frequency";
        //public static readonly string OptionFrequency1Id = "frequencyCodeANNUAL";
        public static readonly string OptionFrequency2Id = "frequencyCodeANNUAL_2";
        //public static readonly string OptionFrequency3Id = "frequencyCodeBIENNIAL";
        //public static readonly string OptionFrequency4Id = "frequencyCodeIRREG";
        #endregion

        #region Qualified Relations
        public static readonly string QualifiedRelationHadRoleId = "qualifiedRelations-select-hadRole";
        public static readonly string QualifiedRelationHrefId = "qualifiedRelations-href";
        public static readonly string QualifiedRelationDeId = "qualifiedRelations-de";
        public static readonly string QualifiedRelationAddRowId = "qualifiedRelations-add-row";
        public static readonly string QualifiedRelationSaveRowId = "qualifiedRelations-save-row";
        public static readonly string QualifiedRelationOptionHadRoleId = "qualifiedRelations-option-hadRole";
        #endregion

        public static readonly string ImportButtonId = "import-dataset";
    }

    public static class ViewMask
    {
        #region Dataset
        public static readonly string DatasetTitleId = "description-title";
        public static readonly string DatasetDescriptionId = "description-description";
        public static readonly string DatasetIdentifierId = "description-identifier";
        public static readonly string DatasetPublisherId = "description-publisher";
        public static readonly string DatasetOwnerId = "description-owner";
        public static readonly string DatasetResponsiblePersonId = "description-responsiblePerson";
        public static readonly string DatasetPublicationDateId = "description-publicationdate";
        public static readonly string DatasetLanguagesId = "description-languages";
        public static readonly string DatasetThemesId = "description-themes";
        public static readonly string DatasetPeriodId = "description-period";
        public static readonly string DatasetLandingPageId = "description-landingpage";
        public static readonly string DatasetAccessRightsId = "description-accessrights";
        public static readonly string DatasetSpatialId = "description-spatial";
        public static readonly string DatasetGeoId = "description-geo-id";
        public static readonly string DatasetCoverageId = "description-coverage";
        public static readonly string DatasetConformsToId = "description-conformtos";
        public static readonly string DatasetReferencedById = "description-referenced-by";
        #endregion

        #region Distributions
        public static readonly string DistributionsTitleId = "distributions-detail-title";
        public static readonly string DistributionsDescriptionId = "distributions-detail-description";
        public static readonly string DistributionsIdentifierId = "distributions-detail-identifier";
        public static readonly string DistributionsReleaseDateId = "distributions-detail-releasedate";
        public static readonly string DistributionsModifiedId = "distributions-detail-modified";
        public static readonly string DistributionsLanguageId = "distributions-detail-language";
        public static readonly string DistributionsAccessUrlId = "distributions-detail-accessurl";
        public static readonly string DistributionsDownloadUrlId = "distributions-detail-downloadurl";
        public static readonly string DistributionsSizeId = "distributions-detail-size";
        public static readonly string DistributionsFormatId = "distributions-detail-format";
        public static readonly string DistributionsMediaTypeId = "distributions-detail-mediatype";
        public static readonly string DistributionsPackagingFormatId = "distributions-detail-packagingformat";
        public static readonly string DistributionsChecksumId = "distributions-detail-checksum";
        public static readonly string DistributionsLicenseId = "distributions-detail-license";
        public static readonly string DistributionsRightsId = "distributions-detail-rights";
        public static readonly string DistributionsAvailabilityId = "distributions-detail-availability";
        public static readonly string DistributionsCoverageFromId = "distributions-detail-coveragefrom";
        public static readonly string DistributionsCoverageToId = "distributions-detail-coverageto";
        public static readonly string DistributionsTemporalResolutionId = "distributions-detail-temporalresolution";
        public static readonly string DistributionsConformsToId = "distributions-detail-conformtos";
        public static readonly string DistributionsDocumentationId = "distributions-detail-documentation";
        public static readonly string DistributionsAccessServicesId = "distributions-detail-accessservices";
        public static readonly string DistributionsImageId = "distributions-detail-image";
        public static readonly string DistributionsFrequencyId = "description-frequency";
        #endregion

        public static readonly string OgdCatalogThemesId = "description-catalogTitle-";

        #region Export
        public static readonly string ExportButtonId = "dataset-export-menubutton";
        public static readonly string ExportJsonId = "export-json";
        #endregion
    }

    public static class Data
    {
        public static readonly string TitleDataset = "Playwright_Test_Dataset";
        public static readonly string IdentifierDataset = "Playwright_Test_Dataset";
        public static readonly string Description = "Playwright_Test : Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam";

        public static readonly string DataOwnerName = "John Doe";
        public static readonly string ResponsiblePersonName = "max.muster@example.org";
        public static readonly string ResponsiblePersonViewDe = "Max Muster";

        public static readonly string ContactPointValue = "Playwright Contact Point";
        public static readonly string ContactAddressValue = "Kirchstrasse 52\n3097 Liebefeld";
        public static readonly string ContactNoteValue = "Seltsam, im Nebel zu wandern!\nEinsam ist jeder Busch und Stein,\nKein Baum sieht den andern,\nJeder ist allein.\n\nVoll von Freunden war mir die Welt,\nAls noch mein Leben licht war;\nNun, da der Nebel fällt,\nIst keiner mehr sichtbar.";
        public static readonly string ContactPointEmailValue = "aaa.bbb@bit.admin.ch";

        public static readonly string KeywordsDeValue = "Playwright Keywords-De";

        public static readonly string LandingPagesHrefValue = "https://www.blick.ch";
        public static readonly string LandingPagesDeIdValue = "Ueber allen Gipfeln ist Ruh',in allen Wipfelnspürest Du kaum einen Hauch";

        public static readonly string SpatialIdValue = "Walle! walle manche Strecke, daß zum Zwecke Wasser fließe";

        public static readonly string ConformsToHrefIdValue = "https://www.watson.ch";
        public static readonly string ConformsToDeIdValue = "In die Ecke, Besen, Besen! seid’s gewesen!";

        public static readonly string IsReferencedByHrefIdValue = "https://www.20min.ch";
        public static readonly string IsReferencedByDeIdValue = "Hat der alte Hexenmeister sich doch einmal wegbegeben! und nun sollen seine Geister auch nach meinem Willen leben.";

        public static readonly string TitleDistribution = "Playwright_Test_Distribution";
        public static readonly string IdentifierDistribution = "Playwright_Test_Distribution";

        public static readonly string AccessUrlDistributionIdValue = "https://www.i14y.ch";
        public static readonly string AccessUrlTitleDeDistributionIdValue = "i14y";

        public static readonly string RightsDistributionIdValue = "Freiheitsrechte, Sozialrechte, Kollektivrechte";
        public static readonly string TemporalResolutionDistributionIdValue = "Von der Stirne heiß rinnen muß der Schweiß, soll das Werk den Meister loben! Doch der Segen kommt von oben";

        public static readonly string DocumentsHrefIdValue = "https://ethz.ch";
        public static readonly string DocumentsDefIdValue = "Molecular Systems Engineering";

        public static readonly string ImageHrefIdValue = "https://ethz.ch";
        public static readonly string ImageDefIdValue = "Molecular Systems Engineering";

        public static readonly string OGDTitleDataset = "Playwright_Test_OGD_Dataset";
        public static readonly string OGDTIdentifierDataset = "Playwright_Test_OGD_Dataset";

        public static readonly string OGDTitleDistribution = "Playwright_Test_OGD_Distribution";
        public static readonly string OGDIdentifierDistribution = "Playwright_Test_OGD_Distribution";

        public static readonly string PublisherViewDe = "I14Y Test Organisation_de";
        public static readonly string ThemesViewDe = "Behörden, Gebäude und Grundstücke, Tiere";
        public static readonly string SizeViewDe = "1 KB";
        public static readonly string ChecksumViewDe = "1234567890";

        public static readonly string NoneView = "-";

        public static readonly string OgdCalolgThemesValueDe1Id = "Vorläufige Daten";
        public static readonly string OgdCalolgThemesValueDe2Id = "Internationale Themen";
        public static readonly string OgdCalolgThemesValueDe3Id = "Bildung, Kultur und Sport";
    }
}