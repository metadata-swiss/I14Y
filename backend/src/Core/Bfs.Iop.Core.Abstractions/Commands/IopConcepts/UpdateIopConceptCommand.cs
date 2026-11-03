using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record UpdateIopConceptCommand(Guid Id, IopConceptInputModel UpdateModel) : IRequest
{ }
