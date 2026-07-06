using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;

public sealed record GetCodeListEntryExistsCommand(Guid ConceptId, string Code) : IRequest<bool>
{ }
