using Bfs.Iop.DataAccess.Abstractions;
using Cerise;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;

public sealed record AddAnnotationCommand(
    Guid ConceptId, 
    Guid CodeListEntryId,
    AnnotationInputModel Model) : IRequest<Guid>
{ }
