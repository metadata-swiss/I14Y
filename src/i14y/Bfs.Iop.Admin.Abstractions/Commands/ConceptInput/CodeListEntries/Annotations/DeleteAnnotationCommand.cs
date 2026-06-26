using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;

public sealed record DeleteAnnotationCommand(
    Guid ConceptId,
    Guid CodeListEntryId,
    Guid AnnotationId) : IRequest
{ }
