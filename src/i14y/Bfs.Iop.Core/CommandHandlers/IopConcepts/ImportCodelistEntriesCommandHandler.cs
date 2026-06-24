using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Serialization.Json;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Serialization.Csv;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ImportCodelistEntriesCommandHandler
    : IRequestHandler<ImportCodelistEntriesCommand>
{
    private readonly IIopConceptsService _iopConceptsService;

    public ImportCodelistEntriesCommandHandler(IIopConceptsService iopConceptsService) => 
        _iopConceptsService = iopConceptsService;

    public async Task Handle(ImportCodelistEntriesCommand request, CancellationToken cancellationToken)
    {
        // Verify if there are no entries
        var pagedResult = await _iopConceptsService.GetCodeListEntries(
            request.ConceptId,
            sortProperty: null,
            SortOrder.Ascending,
            page: 1, pageSize: 1,
            cancellationToken);

        if (pagedResult.TotalCount > 0)
        {
            throw new MethodNotAllowedException("The concept contains entries. Please delete them and try again.");
        }

        var inputModels = request.DataFormat switch
        {
            CodeListEntriesDataFormat.Json => IopJsonSerializer.DeserializeStreamData<IEnumerable<CodeListEntryInputModel>>(request.Data),
            CodeListEntriesDataFormat.Csv => CodeListEntriesCsvSerializer.DeserializeStreamData(request.Data),
            _ => throw new NotImplementedException($"The format '{request.DataFormat}' is not supported.")
        };

        await _iopConceptsService.AddCodeListEntries(
            request.ConceptId,
            inputModels,
            cancellationToken);

        return;
    }
}