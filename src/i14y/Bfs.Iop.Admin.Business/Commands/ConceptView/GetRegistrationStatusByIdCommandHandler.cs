using Bfs.Iop.Admin.Commands.ConceptView;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView;

internal sealed class GetRegistrationStatusByIdCommandHandler : IRequestHandler<GetRegistrationStatusByIdCommand, RegistrationStatusInfoModel>
{
    private readonly IIopCoreApiClient _apiClient;

    public GetRegistrationStatusByIdCommandHandler(IIopCoreApiClient apiClient) => _apiClient = apiClient;

    public async Task<RegistrationStatusInfoModel> Handle(GetRegistrationStatusByIdCommand request, CancellationToken cancellationToken)
    {
        var getRegistrationStatusResponse = await _apiClient.GetConceptsRegistrationStatusByIdAsync(request.ConceptId, cancellationToken);

        return getRegistrationStatusResponse.Result;
    }
}
