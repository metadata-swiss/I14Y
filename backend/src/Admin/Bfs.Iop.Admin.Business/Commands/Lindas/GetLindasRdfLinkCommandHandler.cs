using System;
using System.Threading;
using System.Threading.Tasks;
using Bfs.Iop.Admin.Commands.Lindas;
using Bfs.Iop.Admin.Models.Lindas;
using MediatR;

namespace Bfs.Iop.Admin.Business.Commands.Lindas;

public sealed class GetLindasRdfLinkCommandHandler : IRequestHandler<GetLindasRdfLinkCommand, Uri?>
{
    private readonly ILindasClient _lindasClient;

    public GetLindasRdfLinkCommandHandler(ILindasClient lindasClient)
    {
        _lindasClient = lindasClient ?? throw new ArgumentNullException(nameof(lindasClient));
    }

    public Task<Uri?> Handle(GetLindasRdfLinkCommand request, CancellationToken cancellationToken) =>
        _lindasClient.GetRdfUrlAsync(request.Type, request.Identifier, request.Version, cancellationToken);
}
