namespace Bfs.Iop.DataAccess.Abstractions
{
    public sealed record DcatCatalogModel
    {
        public required MultiLanguageModel Description { get; init; }

        public Guid Id { get; init; }

        public required AgentModel Publisher { get; init; }

        public required SystemInfoModel System {  get; init; }

        public IReadOnlyCollection<string> ThemeTaxonomy { get; init; } = [];

        public required MultiLanguageModel Title { get; init; }
    }
}
