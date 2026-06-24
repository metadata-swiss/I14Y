namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatCatalogThemeInputModel
{
    public required string Code { get; set; }

    public required string ThemeTaxonomy { get; init; }
}
