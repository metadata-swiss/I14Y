using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopConcepts;

public sealed record GetIdentifierVersionExistsCommand(
    string Identifier, 
    string Version) : IRequest<IdentifierVersionExistsModel>
{ }
