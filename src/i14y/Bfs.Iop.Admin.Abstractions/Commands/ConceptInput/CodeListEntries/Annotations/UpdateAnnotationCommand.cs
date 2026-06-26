using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;

public sealed record UpdateAnnotationCommand(
    Guid ConceptId,
    Guid CodeListEntryId,
    Guid AnnotationId,
    Models.Annotation Model) : IRequest
{ }
