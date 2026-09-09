using Bfs.Iop.Core.Abstractions.Commands.IopPersons;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopPersons;

internal class SeedIopPersonsCommandHandler :
    IRequestHandler<SeedIopPersonsCommand>
{
    private readonly IIopPersonsService _iopPersonService;

    public SeedIopPersonsCommandHandler(IIopPersonsService iopPersonService) =>
        _iopPersonService = iopPersonService;

    public async Task Handle(SeedIopPersonsCommand request, CancellationToken cancellationToken)
    {
        foreach (var iopPerson in request.IopPersons)
        {
            await _iopPersonService.AddIopPerson(iopPerson, cancellationToken);
        }
    }
}