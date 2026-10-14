using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Api.Authorization;

internal sealed class SearchCallerFactory : ISearchCallerFactory
{
    private readonly IUserContextService _userContext;

    public SearchCallerFactory(IUserContextService userContext)
    {
        _userContext = userContext;
    }

    public SearchCaller Create()
    {
        if (!_userContext.IsUserTokenValid())
        {
            return SearchCaller.Anonymous;
        }

        return new SearchCaller
        {
            Role = _userContext.GetUserBusinessRole(),
            Agencies = _userContext.GetUserAgencies(),
        };
    }
}
