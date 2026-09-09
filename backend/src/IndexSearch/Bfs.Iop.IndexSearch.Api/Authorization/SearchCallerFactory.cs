using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Api.Authorization;

/// <summary>
///     Turns the caller's token into the role and agencies the index filters by.
/// </summary>
public interface ISearchCallerFactory
{
    SearchCaller Create();
}

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
            Role = ResolveRole(),
            Agencies = _userContext.GetUserAgencies(),
        };
    }


    private BusinessRole ResolveRole()
    {
        if (Has(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService))
        {
            return BusinessRole.InteroperabilityService;
        }

        if (Has(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward))
        {
            return BusinessRole.SwissDataSteward;
        }

        if (Has(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward))
        {
            return BusinessRole.LocalDataSteward;
        }

        if (Has(IopClaimsHelper.Roles.BusinessRoles.Submitter))
        {
            return BusinessRole.Submitter;
        }

        if (Has(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer))
        {
            return BusinessRole.StewardshipOrganisationViewer;
        }

        return BusinessRole.Unknown;
    }

    private bool Has(string role) => _userContext.UserHasRole(role);
}
