using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ExportConceptCommandHandler : IRequestHandler<ExportConceptCommand, ExportFile>
{
    private readonly IIopConceptsService _iopConceptsService;

    public ExportConceptCommandHandler(IIopConceptsService iopConceptsService) =>
        _iopConceptsService = iopConceptsService;

    public async Task<ExportFile> Handle(ExportConceptCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFormat is not DataFormat.Json)
        {
            throw new NotSupportedException($"Data format {request.DataFormat} is not supported for concept export.");
        }

        var concept = await _iopConceptsService.GetIopConcept(request.ConceptId, includeCodeListEntries: true, cancellationToken);

        var filename = $"Concept_{concept.Identifiers.First()}-{concept.Version}";

        return IopJsonSerializer.SerializeToFile(filename, concept);
    }
}
