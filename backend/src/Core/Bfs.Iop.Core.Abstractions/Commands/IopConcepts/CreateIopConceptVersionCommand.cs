using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record CreateIopConceptVersionCommand(Guid PreviousId, IopConceptInputModel Input) : IRequest<Guid>
{}