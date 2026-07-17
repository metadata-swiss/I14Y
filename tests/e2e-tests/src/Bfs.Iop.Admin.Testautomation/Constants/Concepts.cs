namespace Bfs.Iop.Admin.Testautomation.Constants;

public static class Concepts
{
    #region Catalog Navigation
    public static readonly string CatalogMenuButtonId = "catalog-menubutton";
    public static readonly string CreateConceptButtonId = "create-concept";
    public static readonly string CatalogSearchId = "catalog-search";
    public static readonly string CatalogTableId = "catalog-table";
    public static readonly string CatalogTableViewButton = "view-detail-button-";
    #endregion

    #region Basic Concept Information
    public static readonly string NameDeId = "title-concept-de";
    public static readonly string DescriptionDeId = "description-concept-de";
    public static readonly string IdentifierId = "identifiers-description-0";
    public static readonly string VersionId = "version-concept";
    public static readonly string ValidFromId = "validFrom_concept";
    public static readonly string ValidToId = "validTo-concept";
    public static readonly string ResponsiblePersonId = "responsiblePerson-concept";
    public static readonly string ResponsibleDeputyId = "responsibleDeputy-concept";
    public static readonly string PublisherId = "publisher-description";

    #endregion

    #region Theme Codes
    public static readonly string ThemeCodesId = "themeCodes";
    public static readonly string ThemeCodesOption0Id = "109";
    public static readonly string ThemeCodesOption1Id = "121";
    public static readonly string ThemeCodesOption2Id = "119";
    public static readonly string ThemeCodesOption3Id = "120";
    public static readonly string ThemeCodesOption4Id = "118";
    public static readonly string ThemeCodesOption5Id = "117";
    #endregion

    #region Concept Types and Options
    public static readonly string ConceptTypeId = "conceptType";
    public static readonly string ConceptTypeOptionCodeListId = "CodeList";
    public static readonly string ConceptTypeOptionDateId = "Date";
    public static readonly string ConceptTypeOptionNumericId = "Numeric";
    public static readonly string ConceptTypeOptionStringId = "String";
    #endregion

    #region Pattern and Validation
    public static readonly string PatternDateId = "pattern-date";
    public static readonly string PatternStringId = "pattern-string";
    public static readonly string PatternNumericId = "pattern-numeric";
    public static readonly string MinLengthId = "minLength";
    public static readonly string MaxLengthId = "maxLength";
    public static readonly string NbDecimalId = "nbDecimal";
    public static readonly string MeasurementUnitId = "measurementUnit";
    public static readonly string IntervalId = "interval";
    public static readonly string MinValueId = "minValue";
    public static readonly string MaxValueId = "maxValue";
    #endregion

    #region CodeList Configuration
    public static readonly string CodeListEntryValueTypeId = "codeListEntryValueType";
    public static readonly string CodelistEntryValueMaxLengthId = "codelistEntryValueMaxLength";
    public static readonly string CodelistEntryOptionStringId = "String";
    public static readonly string CodelistEntryOptionNumericId = "Numeric";
    #endregion

    #region Keywords and Landing Pages
    public static readonly string KeywordsAddRowId = "keywords-add-row";
    public static readonly string KeywordsDeId = "keywords-de";
    public static readonly string KeywordSaveRowId = "keywords-save-row";
    public static readonly string LandingPagesAddRowId = "landingPages-add-row";
    public static readonly string LandingPagesHrefId = "landingPages-href";
    public static readonly string LandingPagesDefId = "landingPages-de";
    public static readonly string LandingPagesSaveRowId = "landingPages-save-row";
    #endregion

    #region Conformity
    public static readonly string ConformTosAddRowId = "conformsTo-add-row";
    public static readonly string ConformTosHrefId = "conformsTo-href";
    public static readonly string ConformTosDefId = "conformsTo-de";
    public static readonly string ConformTosSaveRowId = "conformsTo-save-row";
    #endregion

    #region CodeList Management
    public static readonly string CodelistImportButtonId = "importbutton";
    public static readonly string CodelistImportJsonButtonId = "importbutton-json";
    public static readonly string CodelistImportCvsButtonId = "importbutton-csv";
    public static readonly string CodelistDeleteAllId = "delete-all-codeList";
    public static readonly string CodelistCheckboxllId = "codelist-checkbox-select-all-rows-input";
    public static readonly string CodelistAddRowId = "codelist-add-row";
    public static readonly string CodelistValueId = "codelist-value";
    public static readonly string CodelistParentId = "codelist-parentCode";
    public static readonly string CodelistCodeNameDeId = "codelist-codename-de";
    public static readonly string CodelistDescriptionDeId = "codelist-description-de";
    public static readonly string CodelistCancelButtonId = "codelist-cancel-button";
    public static readonly string CodelistSumitButtonId = "codelist-submit-button";
    #endregion

    #region CodeList Annotation
    //public static readonly string AnnotationTypeId = "edit-annotation-type";
    //public static readonly string AnnotationTitleId = "edit-annotation-title";
    public static readonly string AnnotationIdentifierId = "edit-annotation-identifier";
    public static readonly string AnnotationTextDeId = "edit-annotation-text-de";
    public static readonly string AnnotationTextEnId = "edit-annotation-text-en";
    public static readonly string AnnotationTextFrId = "edit-annotation-text-fr";
    public static readonly string AnnotationTextItId = "edit-annotation-text-it";
    public static readonly string AnnotationUriId = "edit-annotation-uri";
    public static readonly string AnnotationCancelButtonId = "edit-annotation-cancel-button";
    public static readonly string AnnotationSaveButtonId = "edit-annotation-save-button";
    #endregion CodeList Annotation

    #region Concept Actions
    public static readonly string ConceptSaveAndCloseId = "concept-saveandclose";
    public static readonly string ConceptCancelId = "concept-cancel";
    public static readonly string ConceptEditId = "concept-edit";
    public static readonly string ConceptDeleteId = "concept-delete";
    public static readonly string ConceptGetBackId = "concept-get-back";
    #endregion

    #region New Concept Properties
    public static readonly string ShowMultilingualNewConcept = "show-multilingual-new-concept";
    public static readonly string IdentifierNewConcept = "identifier-new-concept";
    public static readonly string ValidFromNewConcept = "validfrom-new-concept";
    public static readonly string ValidToNewConcept = "validto-new-concept";
    public static readonly string ResponiblePersonNewConcept = "responsible-person-new-concept";
    public static readonly string ResponiblePersonNewConcept0 = "responsible-person-new-concept0";
    public static readonly string ResponibleDeputyNewConcept = "responsible-deputy-new-concept";
    public static readonly string ResponibleDeputyNewConcept0 = "responsible-deputy-new-concept0";
    public static readonly string NameNewConcept = "name-new-concept-de";
    public static readonly string DescriptionNewConcept = "description-new-concept-de";
    public static readonly string CancelNewConcept = "cancel-new-concept";
    #endregion

    #region Control Names
    public static readonly string ControlNameConformTo = "conformsTo";
    public static readonly string ControlNameDocuments = "documents";
    public static readonly string ControlNameImage = "image";
    public static readonly string ControlNameKeywords = "keywords";
    public static readonly string ControlNameLandingPages = "landingPages";
    public static readonly string ControlNameIsReferencedBy = "isReferencedBy";
    public static readonly string ControlNameCodeList = "codeList";
    #endregion

    #region Tree Section
    public static readonly string NavTreeSectionProperiesId = "nav-tree-section-properties";

    #endregion Tree Section

    #region Annotation
    public static readonly string ShowAnnotationGridId = "chevron_";// plus index
    public static readonly string AddAnnotationRowId = "edit-annotation-add-row-button";

    public static readonly string EditAnnotationTypeId = "edit-annotation-type";
    public static readonly string EditAnnotationTitleId = "edit-annotation-title";
    public static readonly string EditAnnotationIdentifierId = "edit-annotation-identifier";
    public static readonly string EditAnnotationTextDeId = "edit-annotation-text-de";
    public static readonly string EditAnnotationUriId = "edit-annotation-uri";

    public static readonly string EditAnnotationCancelButtonId =  "edit-annotation-cancel-button";
    public static readonly string EditAnnotationSaveButtonId = "edit-annotation-save-button";

    #endregion Annotation

    public static readonly string PublisherIdOptionTestOrganisation = "agent0";
}