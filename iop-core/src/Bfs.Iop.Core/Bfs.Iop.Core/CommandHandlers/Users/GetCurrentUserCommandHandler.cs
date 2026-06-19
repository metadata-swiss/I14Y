using Bfs.Iop.Core.Abstractions.Commands.Users;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Users;

internal sealed class GetCurrentUserCommandHandler : IRequestHandler<GetCurrentUserCommand, UserModel>
{
    private readonly IUserContextService _userContextService;

    public GetCurrentUserCommandHandler(IUserContextService userContextService) =>
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));

    public Task<UserModel> Handle(GetCurrentUserCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(
            new UserModel()
            {
                FirstName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType),
                LastName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType),
                Email = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType),
                BusinessRole = _userContextService.GetUserBusinessRole()
            });
}
