using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.IdentifierExists;

internal class ExistsMoreThanOneConceptForIdentifierCommandHandler : IRequestHandler<ExistsMoreThanOneConceptForIdentifierCommand, bool>
{
    private readonly IIopCoreApiClient _apiClient;

    public ExistsMoreThanOneConceptForIdentifierCommandHandler(
        IIopCoreApiClient apiClient) => _apiClient = apiClient;

    public async Task<bool> Handle(ExistsMoreThanOneConceptForIdentifierCommand request, CancellationToken cancellationToken)
    {
        var iopConceptsResponse = await _apiClient.GetConceptsIdentifierByIdentifierAsync(request.Identifier, cancellationToken);

        return iopConceptsResponse.Result.Count > 0;
    }
}