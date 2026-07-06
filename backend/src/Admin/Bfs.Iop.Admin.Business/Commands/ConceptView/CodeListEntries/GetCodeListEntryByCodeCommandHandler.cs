using Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView.CodeListEntries;

internal sealed class GetCodeListEntryByCodeCommandHandler : IRequestHandler<GetCodeListEntryByCodeCommand, CodeListEntryDetail>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetCodeListEntryByCodeCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _apiClient = apiClient;
    }

    public async Task<CodeListEntryDetail> Handle(GetCodeListEntryByCodeCommand request, CancellationToken cancellationToken)
    {
        var getCodeListEntriesByCode = await _apiClient.GetConceptsCodelistEntriesByCodeByIdAndCodeAsync(
            request.ConceptId,
            request.Code,
            cancellationToken);

        return _mapper.Map<CodeListEntryDetail>(getCodeListEntriesByCode.Result);
    }
}
