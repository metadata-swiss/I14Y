namespace Bfs.Iop.Admin.Models;

public sealed record IdentifierVersionExistsResult(
    string Identifier,
    string Version,
    bool Result,
    IdentifierVersionExistsResultMessage Message,
    IdentifierVersionExistsResultObjectType ObjectType)
{ }
