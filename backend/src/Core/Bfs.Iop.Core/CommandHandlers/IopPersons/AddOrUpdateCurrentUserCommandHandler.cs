using Bfs.Iop.Core.Abstractions.Commands.IopPersons;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopPersons;

internal sealed class AddOrUpdateCurrentUserCommandHandler :
    IRequestHandler<AddOrUpdateCurrentUserCommand>
{
    private readonly IIopPersonsService _iopPersonService;

    public AddOrUpdateCurrentUserCommandHandler(
        IIopPersonsService iopPersonService) => _iopPersonService = iopPersonService;

    public Task Handle(AddOrUpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        return _iopPersonService.AddOrUpdateCurrentIopPerson(cancellationToken);
    }
}