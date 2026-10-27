namespace Bfs.Iop.IndexSearch.Contracts.Search;

/// <summary>
///     What a caller gets when it does not ask for a particular page.
/// </summary>
/// <remarks>
///     Shared because the GET and POST routes have to agree. They did not: the records below carried
///     these defaults while the GET parameters were plain <c>int</c>, so an omitted page size bound to
///     0 - a legal size meaning "no hits" - and the route answered with a correct total and an empty
///     list.
/// </remarks>
public static class SearchPaging
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 200;
}
