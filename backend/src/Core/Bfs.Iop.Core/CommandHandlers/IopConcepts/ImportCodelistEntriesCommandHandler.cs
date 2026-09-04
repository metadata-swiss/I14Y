using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Serialization.Csv;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ImportCodelistEntriesCommandHandler
    : IRequestHandler<ImportCodelistEntriesCommand>
{
    private readonly IIopConceptsService _iopConceptsService;
    private readonly ICodeListEntryIndexService _indexService;

    public ImportCodelistEntriesCommandHandler(
        IIopConceptsService iopConceptsService,
        ICodeListEntryIndexService indexService)
    {
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));
        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
    }

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

        var codelistEntries = await _iopConceptsService.GetCodeListEntries(
            request.ConceptId,
            sortProperty: null,
            SortOrder.Ascending,
            page: 1, pageSize: int.MaxValue,
            cancellationToken);

        _indexService.Index(codelistEntries.Results);

        return;
    }
}