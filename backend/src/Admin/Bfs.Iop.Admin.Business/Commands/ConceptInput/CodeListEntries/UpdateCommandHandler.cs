using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput.CodeListEntries;

internal sealed class UpdateCommandHandler : IRequestHandler<UpdateCommand>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public UpdateCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        var readResponse = await _apiClient.GetConceptsCodelistEntriesByIdAndCodeListEntryIdAsync(
            request.ConceptId,
            request.CodeListEntryId,
            cancellationToken);

        var readModel = readResponse.Result;

        var inputModel = _mapper.Map<CodeListEntryInputModel>(request.CodeListEntryInput);
        inputModel.Annotations = _mapper.Map<IEnumerable<AnnotationInputModel>>(readModel.Annotations).ToList();

        await _apiClient.PutConceptsCodelistEntriesByIdAndCodeListEntryIdAndBodyAsync(
            request.ConceptId,
            request.CodeListEntryId,
            inputModel,
            cancellationToken);
    }
}
