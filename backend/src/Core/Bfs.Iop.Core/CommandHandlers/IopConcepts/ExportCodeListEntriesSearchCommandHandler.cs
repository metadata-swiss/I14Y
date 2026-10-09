using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.Core.Search;
using Bfs.Iop.Core.Serialization.Csv;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ExportCodeListEntriesSearchCommandHandler : IRequestHandler<ExportCodeListEntriesSearchCommand, ExportFile>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexSearch _search;

    public ExportCodeListEntriesSearchCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntryIndexSearch search)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _search = search ?? throw new ArgumentNullException(nameof(search));
    }

    public async Task<ExportFile> Handle(ExportCodeListEntriesSearchCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var concept = await _conceptsService.GetIopConcept(request.ConceptId, includeCodeListEntries: false, cancellationToken);

        var entries = await _search.SearchAllAsync(
            request.ConceptId,
            request.Language,
            request.Query,
            request.Filters,
            cancellationToken);

        var dateUtc = DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss");
        var filename = $"CodelistEntries_{concept.Identifiers.First()}-{concept.Version}_searchResults_{dateUtc}";

        return request.DataFormat switch
        {
            CodeListEntriesDataFormat.Json => IopJsonSerializer.SerializeToFile(
                filename,
                entries),
            CodeListEntriesDataFormat.Csv => CodeListEntriesCsvSerializer.SerializeToFile(
                filename,
                entries),
            _ => throw new NotImplementedException($"The format '{request.DataFormat}' is not supported."),
        };
    }
}
