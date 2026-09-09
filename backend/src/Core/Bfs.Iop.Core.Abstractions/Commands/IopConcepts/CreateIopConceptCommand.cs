using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record CreateIopConceptCommand(IopConceptInputModel Input) : IRequest<Guid>
{ }
