using Bfs.Iop.Admin.Commands.ConceptView;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView;

internal sealed class GetPublicationLevelByIdCommandHandler : IRequestHandler<GetPublicationLevelByIdCommand, PublicationLevelInfoModel>
{
    private readonly IIopCoreApiClient _apiClient;

    public GetPublicationLevelByIdCommandHandler(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient;

    public async Task<PublicationLevelInfoModel> Handle(GetPublicationLevelByIdCommand request, CancellationToken cancellationToken)
    {
        var getPublicationLevelResponse = await _apiClient.GetConceptsPublicationLevelByIdAsync(request.ConceptId, cancellationToken);

        return getPublicationLevelResponse.Result;
    }
}