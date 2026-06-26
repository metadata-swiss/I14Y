using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;

public sealed record GetInputCommand(Guid ConceptId, Guid CodeListEntryId) : IRequest<Models.CodelistEntryInput>
{ }
