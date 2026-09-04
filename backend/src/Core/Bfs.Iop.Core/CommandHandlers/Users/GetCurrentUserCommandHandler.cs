using Bfs.Iop.Core.Abstractions.Commands.Users;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Users;

internal sealed class GetCurrentUserCommandHandler : IRequestHandler<GetCurrentUserCommand, UserModel>
{
    private readonly IUserContextService _userContextService;
    private readonly IMediator _mediator;

    public GetCurrentUserCommandHandler(
        IUserContextService userContextService,
        IMediator mediator)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<UserModel> Handle(GetCurrentUserCommand request, CancellationToken cancellationToken) => 
        new()
        {
            FirstName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType),
            LastName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType),
            Email = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType),
            BusinessRole = _userContextService.GetUserBusinessRole(),
            Agents = await GetAgentsIdentifierName(cancellationToken)
        };

    private async Task<IEnumerable<IdentifierNameModel>> GetAgentsIdentifierName(CancellationToken cancellationToken)
    {
        var agents = await _mediator.Send(new GetCurrentUserAgentsCommand(), cancellationToken);

        return [.. agents.Select(x => new IdentifierNameModel
        {
            Identifier = x.Identifier,
            Name = x.Name
        })];
    }
}
