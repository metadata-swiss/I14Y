using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record ExportConceptCommand(Guid ConceptId, DataFormat DataFormat) : IRequest<ExportFile>
{ }
