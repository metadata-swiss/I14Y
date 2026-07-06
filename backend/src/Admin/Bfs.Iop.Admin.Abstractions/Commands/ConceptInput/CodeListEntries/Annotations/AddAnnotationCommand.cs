using Bfs.Iop.Core.Abstractions.Models;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;

public sealed record AddAnnotationCommand(
    Guid ConceptId, 
    Guid CodeListEntryId,
    AnnotationInputModel Model) : IRequest<Guid>
{ }
