using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopPersons;

public record GetIopPersonByEmailCommand(string Email) : IRequest<IopPersonModel>;