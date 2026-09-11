namespace Bfs.Iop.IndexSearch.Elasticsearch;

public sealed class IndexWriteTarget
{
    public IndexWriteTarget(IndexNames names)
    {
        ArgumentNullException.ThrowIfNull(names, nameof(names));

        Catalog = names.Catalog;
        CodeList = names.CodeList;
    }

    public string Catalog { get; private set; }

    public string CodeList { get; private set; }

    public void RedirectTo(string catalog, string codeList)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeList);

        Catalog = catalog;
        CodeList = codeList;
    }
}
