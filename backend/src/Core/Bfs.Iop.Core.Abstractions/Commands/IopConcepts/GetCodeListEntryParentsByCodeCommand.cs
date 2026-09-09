using Bfs.Iop.Core.Abstractions.Models.Search;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetCodeListEntryParentsByCodeCommand(Guid ConceptId, string Code) : IRequest<IEnumerable<CodeListEntrySearchResultPathModel>>
{ }
