using Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView.CodeListEntries;

internal sealed class GetCodeListEntryExistsCommandHandler : IRequestHandler<GetCodeListEntryExistsCommand, bool>
{
    private readonly IIopCoreApiClient _apiClient;

    public GetCodeListEntryExistsCommandHandler(
        IIopCoreApiClient apiClient) => _apiClient = apiClient;

    public async Task<bool> Handle(GetCodeListEntryExistsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _ = await _apiClient.GetConceptsCodelistEntriesByCodeByIdAndCodeAsync(
                request.ConceptId,
                request.Code,
                cancellationToken);

            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }
}
