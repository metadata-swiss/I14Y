namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

internal static class DefaultScoring
{
    public static CatalogScoring Catalog => new SearchScoringOptions().Catalog;

    public static CodeListScoring CodeList => new SearchScoringOptions().CodeList;
}
