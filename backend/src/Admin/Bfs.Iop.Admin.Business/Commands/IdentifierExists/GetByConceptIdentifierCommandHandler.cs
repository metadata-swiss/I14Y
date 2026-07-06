using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.IdentifierExists;

internal class GetByConceptIdentifierCommandHandler : IRequestHandler<GetByConceptIdentifierCommand, bool>
{
    private readonly IIopCoreApiClient _apiClient;

    public GetByConceptIdentifierCommandHandler(
        IIopCoreApiClient apiClient) => _apiClient = apiClient;

    public async Task<bool> Handle(GetByConceptIdentifierCommand request, CancellationToken cancellationToken)
    {
        var iopConceptsByIdentifierResponse = await _apiClient.GetConceptsIdentifierByIdentifierAsync(request.Identifier, cancellationToken);

        return iopConceptsByIdentifierResponse.Result.Count > 0;
    }
}