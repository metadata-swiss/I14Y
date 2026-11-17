namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogInputModel
{
    public required MultiLanguageModel Description { get; init; }

    public required IdentifierNameInputModel Publisher { get; init; }

    public required IdentifierInputModel Tenant { get; init; }

    public IReadOnlyCollection<string> ThemeTaxonomy { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }
}
