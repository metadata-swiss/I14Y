using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.Core.Serialization.Csv;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ExportCodeListEntriesCommandHandler
    : IRequestHandler<ExportCodeListEntriesCommand, ExportFile>
{
    private readonly IIopConceptsService _iopConceptsService;

    public ExportCodeListEntriesCommandHandler(IIopConceptsService iopConceptsService) => 
        _iopConceptsService = iopConceptsService;

    public async Task<ExportFile> Handle(ExportCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        var concept = await _iopConceptsService.GetIopConcept(request.ConceptId, true, cancellationToken);

        var codeListEntries = concept.CodeListEntries?
            .Select(x => request.WithAnnotations ? x : x with { Annotations = null })
            .ToList();

        var filename = $"CodelistEntries_{concept.Identifiers.First()}-{concept.Version}";

        return request.DataFormat switch
        {
            CodeListEntriesDataFormat.Json => IopJsonSerializer.SerializeToFile(filename, codeListEntries!),
            CodeListEntriesDataFormat.Csv => CodeListEntriesCsvSerializer.SerializeToFile(filename, codeListEntries!),
            _ => throw new NotImplementedException($"The format '{request.DataFormat}' is not supported."),
        };
    }
}
