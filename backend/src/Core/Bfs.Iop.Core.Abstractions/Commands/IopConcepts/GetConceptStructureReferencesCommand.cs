using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetConceptStructureReferencesCommand(Guid ConceptId, int? Page, int? PageSize) : IRequest<PagedResult<IopConceptStructureReferenceModel>>
{}
