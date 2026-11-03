using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record ExportCodeListEntriesSearchCommand(
    Guid ConceptId,
    string Language,
    string? Query,
    List<string> Filters,
    CodeListEntriesDataFormat DataFormat) : IRequest<ExportFile>
{ }
