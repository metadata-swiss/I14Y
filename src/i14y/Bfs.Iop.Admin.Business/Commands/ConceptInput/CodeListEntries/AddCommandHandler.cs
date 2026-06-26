using AutoMapper;
using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput.CodeListEntries;

internal class AddCommandHandler : IRequestHandler<AddCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public AddCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Guid> Handle(AddCommand request, CancellationToken cancellationToken)
    {
        var input = _mapper.Map<CodeListEntryInputModel>(request.CodeListEntryInput);
        var response = await _apiClient.PostConceptsCodelistEntriesByIdAndBodyAsync(request.ConceptId, [input], cancellationToken);

        return response.Result.Single();
    }
}