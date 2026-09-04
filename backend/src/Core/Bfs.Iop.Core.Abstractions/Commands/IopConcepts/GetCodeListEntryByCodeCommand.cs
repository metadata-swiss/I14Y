using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntryByCodeCommand(Guid ConceptId, string Code) : IRequest<CodeListEntryModel>
{ }
