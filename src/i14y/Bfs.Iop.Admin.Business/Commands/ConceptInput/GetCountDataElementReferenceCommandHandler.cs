using Bfs.Iop.Admin.Commands.ConceptInput;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput;

internal class GetCountDataElementReferenceCommandHandler : IRequestHandler<GetCountDataElementReferenceCommand, int>
{
    public Task<int> Handle(GetCountDataElementReferenceCommand request, CancellationToken cancellationToken)
    {
        // ToDo: Implement it properly in Iop Core.

        return Task.FromResult(0);
    }
}