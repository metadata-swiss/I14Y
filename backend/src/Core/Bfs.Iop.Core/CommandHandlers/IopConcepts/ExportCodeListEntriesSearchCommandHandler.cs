using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.Core.Serialization.Csv;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ExportCodeListEntriesSearchCommandHandler : IRequestHandler<ExportCodeListEntriesSearchCommand, ExportFile>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntrySearchService _codeListEntrySearchService;

    public ExportCodeListEntriesSearchCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntrySearchService codeListEntrySearchService)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

        _codeListEntrySearchService = codeListEntrySearchService ??
                throw new ArgumentNullException(nameof(codeListEntrySearchService));
    }

    public async Task<ExportFile> Handle(ExportCodeListEntriesSearchCommand request, CancellationToken cancellationToken)
    {
        var concept = await _conceptsService.GetIopConcept(request.ConceptId, includeCodeListEntries: false, cancellationToken);

        var searchResults = await _codeListEntrySearchService.Search(
            request.ConceptId,
            request.Language,
            request.Query,
            request.Filters,
            addCodeListEntriesPaths: false,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        var dateUtc = DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss");
        var filename = $"CodelistEntries_{concept.Identifiers.First()}-{concept.Version}_searchResults_{dateUtc}";

        var entries = searchResults.Results.Select(x => x.Entry);

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
