using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Api.Authorization;

/// <summary>
///     Turns the caller's token into the role and agencies the index filters by.
/// </summary>
public interface ISearchCallerFactory
{
    SearchCaller Create();
}
