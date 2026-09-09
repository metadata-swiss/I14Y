namespace Bfs.Iop.Admin.Testautomation.Constants;

public static class Navigation
{
    public static readonly string UserId = "user";
    public static readonly string AccessTokenId = "access-token";
    public static readonly string AccessTokenKey = "access_token";
    public static readonly string NavigationLinks = "#navigation li a";
    public static readonly string HomeUrlSubString = "/home";
    public static readonly string CatalogUrlSubString = "/catalog";
    public static readonly string ConceptUrlSubString = "/concepts";

    public static readonly string HomeTag = "app-home";
    public static readonly string CatalogTag = "app-catalog";
    public static readonly string ConceptTag = "app-concepts";

    public static readonly string CatalogId = "i18n-navigation-catalog-catalog";

    public static readonly string LanguageDropdownId = "ob-language-dropdown";
    public static readonly string LanguageOptionDeId = "ob-language-de-option";
    public static readonly string LanguageOptionFrId = "ob-language-fr-option";
    public static readonly string LanguageOptionItId = "ob-language-it-option";
    public static readonly string LanguageOptionEnId = "ob-language-en-option";

    public static readonly string[] HomePages = { "Startseite", "Page d’accueil", "Pagina iniziale", "Home" };

    public static readonly string LanguageButtonsCssSelector = "ul[aria-labelledby='ob-language-change'] button.ob-master-layout-header-toggle.ob-control-locale";
    public static readonly string NavigationLinkCssSelector = "a.mat-tooltip-trigger[href='/home']";
    public static readonly string NavigationLinkId = "i18n-navigation-home-home";  

}
