namespace Bfs.Iop.Core.Data.Entities;

internal class CheckSum : EntityBase
{
    public string? Algorithm { get; set; } = null!;

    public string CheckSumValue { get; set; } = null!;

    public Distribution DistributionChecksum { get; set; } = null!;

    public Guid? DistributionChecksumId { get; set; }
}