using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts;

public static class IndexLanguages
{
    public const string Default = MultiLanguageModel.GermanKey;

    public static IReadOnlyList<string> All { get; } =
    [
        MultiLanguageModel.GermanKey,
        MultiLanguageModel.EnglishKey,
        MultiLanguageModel.FrenchKey,
        MultiLanguageModel.ItalianKey,
        MultiLanguageModel.RomanshKey,
    ];
}
