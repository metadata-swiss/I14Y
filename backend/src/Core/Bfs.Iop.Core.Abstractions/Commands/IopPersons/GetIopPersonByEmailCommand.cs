using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.IopPersons;

public record GetIopPersonByEmailCommand(string Email) : IRequest<IopPersonModel>;