namespace Bfs.Iop.DataAccess.Relational.Entities;

internal sealed class PeriodOfTime : EntityBase
{
    public DateTimeOffset? End { get; set; }

    public DateTimeOffset? Start { get; set; }

    public Dataset? TemporalCoverage { get; set; }

    public Guid? TemporalCoverageId { get; set; }

    public Distribution? DistributionCoverage { get; set; }

    public Guid? DistributionCoverageId { get; set; }
}