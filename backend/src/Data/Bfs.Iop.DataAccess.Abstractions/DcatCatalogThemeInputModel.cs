namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogThemeInputModel
{
    public required string Code { get; set; }

    public required string ThemeTaxonomy { get; init; }
}
