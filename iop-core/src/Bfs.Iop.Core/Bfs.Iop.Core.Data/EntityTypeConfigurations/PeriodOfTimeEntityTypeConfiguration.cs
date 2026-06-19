using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class PeriodOfTimeEntityTypeConfiguration : EntityTypeConfiguration<PeriodOfTime>
{
    public override void Configure(EntityTypeBuilder<PeriodOfTime> builder)
    {
        builder.ToTable(nameof(PeriodOfTime).ToSnakeCase());

        builder.Property(e => e.End).AddLocalDateTimeOffsetConversion();
        builder.Property(e => e.Start).AddLocalDateTimeOffsetConversion();
    }
}