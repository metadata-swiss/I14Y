using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class MultiLanguage
{
    public string? De { get; set; }

    public string? En { get; set; }

    public string? Fr { get; set; }

    public string? It { get; set; }

    public string? Rm { get; set; }

    public static MultiLanguage? BuildFromDictionary(IDictionary<string, string> values)
    {
        if (values.Count == 0) return null;

        return new MultiLanguage
        {
            De = values.TryGetValue("de", out var deValue) ? deValue : null,
            En = values.TryGetValue("en", out var enValue) ? enValue : null,
            Fr = values.TryGetValue("fr", out var frValue) ? frValue : null,
            It = values.TryGetValue("it", out var itValue) ? itValue : null,
            Rm = values.TryGetValue("rm", out var rmValue) ? rmValue : null,
        };
    }

    public bool IsEmpty()
                => string.IsNullOrEmpty(De) && string.IsNullOrEmpty(En) && string.IsNullOrEmpty(Fr) && string.IsNullOrEmpty(It) && string.IsNullOrEmpty(Rm);

    public Dictionary<string, string> ToDictionary()
    {
        var result = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(De)) result["de"] = De;
        if (!string.IsNullOrEmpty(En)) result["en"] = En;
        if (!string.IsNullOrEmpty(Fr)) result["fr"] = Fr;
        if (!string.IsNullOrEmpty(It)) result["it"] = It;
        if (!string.IsNullOrEmpty(Rm)) result["rm"] = Rm;

        return result;
    }
}