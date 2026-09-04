namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record PublicationLevelInfoModel
{
    public IReadOnlyCollection<PublicationLevel> AllowedLevels { get; init; } = [];

    public IReadOnlyCollection<PublicationLevel> AllowedProposals { get; init; } = [];

    public bool CanUserRevertProposal { get; init; }

    public PublicationLevel Level { get; init; }

    public PublicationLevel? Proposal { get; init; }
}
