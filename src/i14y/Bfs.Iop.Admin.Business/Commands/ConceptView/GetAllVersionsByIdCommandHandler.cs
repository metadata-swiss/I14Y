using Bfs.Iop.Admin.Commands.ConceptView;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView;

internal sealed class GetAllVersionsByIdCommandHandler 
    : IRequestHandler<GetAllVersionsByIdCommand, IEnumerable<ConceptVersionView>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetAllVersionsByIdCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<ConceptVersionView>> Handle(GetAllVersionsByIdCommand request, CancellationToken cancellationToken)
    {
        var getIopConceptVersionsResponse = await _apiClient.GetConceptsVersionsByIdAsync(request.ConceptId, cancellationToken);

        return _mapper.Map<IEnumerable<ConceptVersionView>>(getIopConceptVersionsResponse.Result);
    }
}