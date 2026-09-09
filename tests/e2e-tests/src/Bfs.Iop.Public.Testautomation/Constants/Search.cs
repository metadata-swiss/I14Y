namespace Bfs.Iop.Public.Testautomation.Constants;

public static class Search
{
    public static readonly string CatalogTabAllId = "catalog-tabs-all";
    public static readonly string CatalogTabDatasetsId = "catalog-tabs-datasets";
    public static readonly string CatalogTabPublicServicesId = "catalog-tabs-publicservices";
    public static readonly string CatalogTabConceptsId = "catalog-tabs-concepts";
    public static readonly string CatalogTabDataServicesId = "catalog-tabs-dataservices";
    public static readonly string CatalogTabOpendataId = "catalog-tabs-opendata";
    public static readonly string CatalogTabGeocatId = "catalog-tabs-geocat";

    public static readonly string FilterVisibilityButtonId = "filter-visibility-button";
    public static readonly string FilterShowTableButtonId = "filter-visibility-show-table-button";
    public static readonly string FilterShowListButtonId = "filter-visibility-show-list";

    public static readonly string FilterDropdownPublisherId = "filter-multiselect-dropdown-publisher";
    public static readonly string FilterDropdownThemesId = "filter-multiselect-dropdown-themes";
    public static readonly string FilterDropdownRegistrationStatusesId = "filter-multiselect-dropdown-registrationStatuses";
    public static readonly string FilterDropdownRegistrationAccessRightsId = "filter-multiselect-dropdown-accessRights";

    public static readonly string FilterFilterChipRemoveallId = "filter-chip-removeall";

    public static readonly string CatalogTableId =  "catalog-search-table";
    public static readonly string CatalogListId = "catalog-search-list";

    public static readonly string CatalogSearchId = "search-box-input";
    public static readonly string CatalogTableViewButton = "view-detail-button-";

    public enum SearchType
    {
        All,
        Datasets,
        PublicServices,
        Api,
        Concepts,
        Opendata,
        Geocat
    }

}
