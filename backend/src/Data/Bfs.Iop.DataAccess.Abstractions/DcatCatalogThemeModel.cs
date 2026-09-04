namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogThemeModel
{
    public required string Code { get; init; }

    public MultiLanguageModel? Name { get; init; }

    public required string ThemeTaxonomy { get; init; }

    public string? Uri { get; init; }
}
