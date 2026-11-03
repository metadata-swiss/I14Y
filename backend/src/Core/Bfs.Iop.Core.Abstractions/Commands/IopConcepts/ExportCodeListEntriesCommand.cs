using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
public sealed record ExportCodeListEntriesCommand(
    Guid ConceptId,
    CodeListEntriesDataFormat DataFormat,
    bool WithAnnotations) : IRequest<ExportFile>
{ }
