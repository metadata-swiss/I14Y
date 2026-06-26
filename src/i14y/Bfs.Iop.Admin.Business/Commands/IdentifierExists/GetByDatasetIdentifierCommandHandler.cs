using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.IdentifierExists;

internal class GetByDatasetIdentifierCommandHandler : IRequestHandler<GetByDatasetIdentifierCommand, bool>
{
    private readonly IIopCoreApiClient _apiClient;

    public GetByDatasetIdentifierCommandHandler(
        IIopCoreApiClient apiClient)
        => _apiClient = apiClient;

    public async Task<bool> Handle(GetByDatasetIdentifierCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _ = await _apiClient.GetDatasetsByIdentifierByIdentifierAsync(request.Identifier, cancellationToken);
            return true;
        }
        catch (ApiException ex) 
        {
            if (ex.StatusCode is 
                StatusCodes.Status401Unauthorized or 
                StatusCodes.Status403Forbidden or 
                StatusCodes.Status404NotFound)
            {
                return ex.StatusCode is not StatusCodes.Status404NotFound;
            }

            throw;
        }
    }
}