using AutoMapper;
using Bfs.Iop.Admin.Commands.VocabularyEntryView;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.VocabularyEntryView;

internal class GetAllByIdentifierCommandHandler : IRequestHandler<GetAllByIdentifierCommand, IEnumerable<Models.VocabularyEntry>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetAllByIdentifierCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<Models.VocabularyEntry>> Handle(GetAllByIdentifierCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetVocabulariesByIdentifierAsync(request.Identifier, cancellationToken);

        return response.Result.Entries.Select(ve => _mapper.Map<Models.VocabularyEntry>(ve));
    }
}