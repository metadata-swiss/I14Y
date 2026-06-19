using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntryByCodeCommand(Guid ConceptId, string Code) : IRequest<CodeListEntryModel>
{ }
