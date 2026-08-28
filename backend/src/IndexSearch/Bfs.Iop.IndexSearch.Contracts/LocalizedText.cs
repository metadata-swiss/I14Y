namespace Bfs.Iop.IndexSearch.Contracts;

public sealed record LocalizedText
{
    public const string German = "de";
    public const string English = "en";
    public const string French = "fr";
    public const string Italian = "it";
    public const string Romansh = "rm";

    public static readonly IReadOnlyList<string> Languages = [German, English, French, Italian, Romansh];

    public string? De { get; init; }

    public string? En { get; init; }

    public string? Fr { get; init; }

    public string? It { get; init; }

    public string? Rm { get; init; }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(De) &&
        string.IsNullOrWhiteSpace(En) &&
        string.IsNullOrWhiteSpace(Fr) &&
        string.IsNullOrWhiteSpace(It) &&
        string.IsNullOrWhiteSpace(Rm);

    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var values = new Dictionary<string, string>(Languages.Count, StringComparer.Ordinal);

        Add(values, German, De);
        Add(values, English, En);
        Add(values, French, Fr);
        Add(values, Italian, It);
        Add(values, Romansh, Rm);

        return values;

        static void Add(Dictionary<string, string> target, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                target[key] = value;
            }
        }
    }
}
