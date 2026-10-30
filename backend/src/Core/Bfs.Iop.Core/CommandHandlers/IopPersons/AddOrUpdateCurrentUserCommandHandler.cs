using Bfs.Iop.Core.Abstractions.Commands.IopPersons;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.IopPersons;

internal sealed class AddOrUpdateCurrentUserCommandHandler :
    AsyncRequestHandler<AddOrUpdateCurrentUserCommand>
{
    private readonly IIopPersonsService _iopPersonService;

    public AddOrUpdateCurrentUserCommandHandler(
        IIopPersonsService iopPersonService) => _iopPersonService = iopPersonService;

    protected override Task Handle(AddOrUpdateCurrentUserCommand request, CancellationToken cancellationToken) => 
        _iopPersonService.AddOrUpdateCurrentIopPerson(cancellationToken);
}