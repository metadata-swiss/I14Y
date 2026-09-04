using Bfs.Iop.Core.Abstractions.Commands.Users;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Users;

internal sealed class GetCurrentUserAgentsCommandHandler : IRequestHandler<GetCurrentUserAgentsCommand, IEnumerable<AgentModel>>
{
    private readonly IUserContextService _userContextService;
    private readonly IAgentsService _agentsService;

    public GetCurrentUserAgentsCommandHandler(
        IUserContextService userContextService,
        IAgentsService agentsService)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
    }

    public async Task<IEnumerable<AgentModel>> Handle(GetCurrentUserAgentsCommand request, CancellationToken cancellationToken)
    {
        if (!_userContextService.IsUserTokenValid())
        {
            return [];
        }

        var businessRole = _userContextService.GetUserBusinessRole();

        if (businessRole is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward)
        {
            return (await _agentsService.GetAgents(identifier: null, uid: null, page: 1, pageSize: int.MaxValue, cancellationToken)).Results;
        }
        
        var agenciesIdentifiers = _userContextService.GetUserAgencies();

        return await _agentsService.GetAgents(agenciesIdentifiers, cancellationToken);
    }
}
