namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatCatalogModel
{
    public required MultiLanguageModel Description { get; init; }

    public Guid Id { get; init; }

    public required AgentModel Publisher { get; init; }

    public required SystemInfoModel System {  get; init; }

    public IEnumerable<string> ThemeTaxonomy { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }
}
