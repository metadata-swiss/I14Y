using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;

public sealed record UpdateCommand(Guid ConceptId, Guid CodeListEntryId, Models.CodelistEntryInput CodeListEntryInput)
    : IRequest
{ }
