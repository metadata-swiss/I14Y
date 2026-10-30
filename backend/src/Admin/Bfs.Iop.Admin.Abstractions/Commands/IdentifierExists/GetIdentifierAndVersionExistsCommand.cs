using Bfs.Iop.Admin.Models;
using Cerise;

namespace Bfs.Iop.Admin.Commands.IdentifierExists;

public sealed record GetIdentifierAndVersionExistsCommand(
    IdentifierVersionExistsResultObjectType ObjectType,
    string Identifier,
    string Version) : IRequest<IdentifierVersionExistsResult>
{ }