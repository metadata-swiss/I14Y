namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record PublicationLevelInfoModel
{
    public IEnumerable<PublicationLevel> AllowedLevels { get; init; } = [];

    public IEnumerable<PublicationLevel> AllowedProposals { get; init; } = [];

    public bool CanUserRevertProposal { get; init; }

    public PublicationLevel Level { get; init; }

    public PublicationLevel? Proposal { get; init; }
}
