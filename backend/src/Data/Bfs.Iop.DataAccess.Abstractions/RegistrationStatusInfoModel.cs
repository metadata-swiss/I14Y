namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record RegistrationStatusInfoModel
{
    public IReadOnlyCollection<RegistrationStatus> AllowedProposals { get; init; } = [];

    public IReadOnlyCollection<RegistrationStatus> AllowedStatuses { get; init; } = [];

    public bool CanUserRevertProposal { get; init; }

    public RegistrationStatus? Proposal { get; init; }

    public RegistrationStatus Status { get; init; }
}
