namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatCatalogInputModel
{
    public required MultiLanguageModel Description { get; init; }

    public required MultiLanguageModel Title { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public IEnumerable<string> ThemeTaxonomy { get; init; } = [];
}
