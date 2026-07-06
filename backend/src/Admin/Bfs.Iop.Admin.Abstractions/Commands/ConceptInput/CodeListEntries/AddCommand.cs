using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;

public sealed record AddCommand(Guid ConceptId, Models.CodelistEntryInput CodeListEntryInput)
    : IRequest<Guid>
{ }