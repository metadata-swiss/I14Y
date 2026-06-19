using Bfs.Iop.Core.Abstractions.Commands.IopPersons;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopPersons;

internal class GetIopPersonByEmailCommandHandler : 
    IRequestHandler<GetIopPersonByEmailCommand, IopPersonModel>
{
    private readonly IIopPersonsService _iopPersonService;

    public GetIopPersonByEmailCommandHandler(
        IIopPersonsService iopPersonService) => _iopPersonService = iopPersonService;

    public async Task<IopPersonModel> Handle(
        GetIopPersonByEmailCommand request,
        CancellationToken cancellationToken)
            => await _iopPersonService.GetIopPersonByEmail(request.Email, cancellationToken);
}