using System;
using System.Threading;
using System.Threading.Tasks;
using Bfs.Iop.Admin.Commands.Lindas;
using Bfs.Iop.Admin.Lindas.Abstractions;
using MediatR;

namespace Bfs.Iop.Admin.Business.Commands.Lindas;

public sealed class GetLindasLinkCommandHandler : IRequestHandler<GetLindasLinkCommand, Uri?>
{
    private readonly ILindasClient _lindasClient;

    public GetLindasLinkCommandHandler(ILindasClient lindasClient)
    {
        _lindasClient = lindasClient ?? throw new ArgumentNullException(nameof(lindasClient));
    }

    public Task<Uri?> Handle(GetLindasLinkCommand request, CancellationToken cancellationToken) =>
        _lindasClient.GetLinkAsync(
            request.LinkType,
            request.Type,
            request.Identifier,
            request.Version,
            cancellationToken);
}