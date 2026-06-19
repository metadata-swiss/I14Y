namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record RegistrationStatusInfoModel
{
    public IEnumerable<RegistrationStatus> AllowedProposals { get; init; } = [];

    public IEnumerable<RegistrationStatus> AllowedStatuses { get; init; } = [];

    public bool CanUserRevertProposal { get; init; }

    public RegistrationStatus? Proposal { get; init; }

    public RegistrationStatus Status { get; init; }
}
