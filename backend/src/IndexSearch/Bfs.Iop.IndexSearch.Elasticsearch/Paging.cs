namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class Paging
{

    public const int MaxResultWindow = 10_000;

    public static (int From, int Size) ToWindow(int page, int pageSize)
    {
        var safePage = Math.Max(page, 1);

        var safeSize = Math.Clamp(pageSize, 0, MaxResultWindow);

        var from = (long)(safePage - 1) * safeSize;

        if (from >= MaxResultWindow)
        {
            return (0, 0);
        }

        return ((int)from, (int)Math.Min(safeSize, MaxResultWindow - from));
    }
}
