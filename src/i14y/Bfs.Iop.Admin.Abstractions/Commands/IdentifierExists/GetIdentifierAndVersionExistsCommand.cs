using Bfs.Iop.Admin.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.IdentifierExists;

public sealed record GetIdentifierAndVersionExistsCommand(
    IdentifierVersionExistsResultObjectType ObjectType,
    string Identifier,
    string Version) : IRequest<IdentifierVersionExistsResult>
{ }