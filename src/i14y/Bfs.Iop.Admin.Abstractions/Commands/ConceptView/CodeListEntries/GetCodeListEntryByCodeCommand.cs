using Bfs.Iop.Admin.Models;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView.CodeListEntries;

public sealed record GetCodeListEntryByCodeCommand(
    Guid ConceptId,
    string Code) : IRequest<CodeListEntryDetail>
{ }
