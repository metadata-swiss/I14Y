namespace Bfs.Iop.Admin.Models;

public class Vcard
{
    public MultiLanguage? AdrWork { get; set; }

    public required string EmailInternet { get; set; }

    public MultiLanguage? Fn { get; set; }

    public MultiLanguage? Note { get; set; }

    public MultiLanguage? Org { get; set; }

    public string? TelWorkVoice { get; set; }
}