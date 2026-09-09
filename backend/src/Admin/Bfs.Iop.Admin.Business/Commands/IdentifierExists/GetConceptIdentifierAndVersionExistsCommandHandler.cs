using Bfs.Iop.Admin.Commands.IdentifierExists;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.Security.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.IdentifierExists;

internal class GetIdentifierAndVersionExistsCommandHandler : IRequestHandler<GetIdentifierAndVersionExistsCommand, IdentifierVersionExistsResult>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IUserContextService _userContextService;

    public GetIdentifierAndVersionExistsCommandHandler(
        IIopCoreApiClient apiClient,
        IUserContextService userContextService)
    {
        _apiClient = apiClient;
        _userContextService = userContextService;
    }

    public async Task<IdentifierVersionExistsResult> Handle(GetIdentifierAndVersionExistsCommand request, CancellationToken cancellationToken)
    {
        var result = new IdentifierVersionExistsResult(
            request.Identifier,
            request.Version,
            false,
            IdentifierVersionExistsResultMessage.None,
            request.ObjectType);

        var task = request.ObjectType switch
        {
            IdentifierVersionExistsResultObjectType.Concept =>
                _apiClient.GetConceptsExistsByIdentifierAndVersionAsync(request.Identifier, request.Version, cancellationToken),
            IdentifierVersionExistsResultObjectType.MappingTable =>
                _apiClient.GetMappingTablesExistsByIdentifierAndVersionAsync(request.Identifier, request.Version, cancellationToken),
            _ => throw new System.NotSupportedException($"The object type '{request.ObjectType}' is not supported"),
        };

        var identifierVersionExists = (await task).Result;

        if (identifierVersionExists.IdentifierExists)
        {
            if (!_userContextService.UserBelongsToAgency(identifierVersionExists.Publisher!.Identifier))
            {
                return result with
                {
                    Result = true,
                    Message = IdentifierVersionExistsResultMessage.IdentifierFromAnotherPublisherAlreadyExists
                };
            }
            else if (identifierVersionExists.VersionExists)
            {
                return result with
                {
                    Result = true,
                    Message = IdentifierVersionExistsResultMessage.IdentifierAndVersionAlreadyExists
                };
            }
        }      
        
        return result;
    }    
}