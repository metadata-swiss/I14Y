namespace Bfs.Iop.Test.Abstraction.Constants;

public static class Eiam
{
    #region Elements 

    public static readonly string FedLoginId = "urn:eiam.admin.ch:idp:e-id:CH-LOGIN";
    public static readonly string CHLoginId = "#ChLoginV2";
    public static readonly string InputEmailId = "isiwebuserid";
    public static readonly string InputPasswordId = "isiwebpasswd";
    public static readonly string InputTanId = "tanresponse";
    public static readonly string LogoutButtonId = "logout";
    public static readonly string SubTitleId = "subtitle";
    public static readonly string TitleId = "title";

    #endregion Elements 

    #region CSS selectors

    public static readonly string ContinueButtonCss = "button span.mdc-button__label";
    public static readonly string CardContainerCss = ".title-container";
    public static readonly string CardListCss = "[role=\"listitem\"]";
    public static readonly string CardCss = "div.card";

    #endregion CSS selectors

    #region Hack

    public static readonly string UrlAutoLogOn = "https://cookie.eiam.admin.ch/autologon";
    public static readonly string ToggleAutoLogOnRef = "AUTOLOGON_REF";
    public static readonly string ToggleAutoLogOnAbn = "AUTOLOGON_ABN";

    #endregion Hack
}
