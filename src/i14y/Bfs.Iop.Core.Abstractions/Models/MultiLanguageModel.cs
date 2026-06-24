namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record MultiLanguageModel
{
    public const string GermanKey = "de";
    public const string EnglishKey = "en";
    public const string FrenchKey = "fr";
    public const string ItalianKey = "it";
    public const string RomanshKey = "rm";

    public string? De { get; set; }

    public string? En { get; set; }

    public string? Fr { get; set; }

    public string? It { get; set; }

    public string? Rm { get; set; }

    public static MultiLanguageModel FromDictionary(IDictionary<string, string> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        return new()
        {
            De = dictionary.TryGetValue(GermanKey, out var deValue) ? deValue : null,
            En = dictionary.TryGetValue(EnglishKey, out var enValue) ? enValue : null,
            Fr = dictionary.TryGetValue(FrenchKey, out var frValue) ? frValue : null,
            It = dictionary.TryGetValue(ItalianKey, out var itValue) ? itValue : null,
            Rm = dictionary.TryGetValue(RomanshKey, out var rmValue) ? rmValue : null,
        };
    }

    public bool IsContentNullOrWhiteSpace() =>
        string.IsNullOrWhiteSpace(De) &&
        string.IsNullOrWhiteSpace(En) && 
        string.IsNullOrWhiteSpace(Fr) && 
        string.IsNullOrWhiteSpace(It) && 
        string.IsNullOrWhiteSpace(Rm);

    public Dictionary<string, string> ToDictionary()
    {
        var result = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(De))
        {
            result.Add(GermanKey, De);
        }

        if (!string.IsNullOrWhiteSpace(Fr))
        {
            result.Add(FrenchKey, Fr);
        }

        if (!string.IsNullOrWhiteSpace(En))
        {
            result.Add(EnglishKey, En);
        }

        if (!string.IsNullOrWhiteSpace(It))
        {
            result.Add(ItalianKey, It);
        }

        if (!string.IsNullOrWhiteSpace(Rm))
        {
            result.Add(RomanshKey, Rm);
        }

        return result;
    }
}
