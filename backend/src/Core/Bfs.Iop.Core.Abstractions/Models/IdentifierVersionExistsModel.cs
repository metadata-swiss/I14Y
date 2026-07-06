namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IdentifierVersionExistsModel
{
    public bool IdentifierExists { get; init; }

    public bool VersionExists { get; init; }

    public AgentModel? Publisher { get; init; }
}
