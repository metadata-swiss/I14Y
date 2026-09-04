using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopPersons;

public sealed record SeedIopPersonsCommand(IEnumerable<IopPersonModel> IopPersons) : IRequest;