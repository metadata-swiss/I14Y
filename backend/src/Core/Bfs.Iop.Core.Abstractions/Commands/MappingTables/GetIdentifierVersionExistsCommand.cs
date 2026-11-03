using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record GetIdentifierVersionExistsCommand(
    string Identifier,
    string Version) : IRequest<IdentifierVersionExistsModel>
{ }
