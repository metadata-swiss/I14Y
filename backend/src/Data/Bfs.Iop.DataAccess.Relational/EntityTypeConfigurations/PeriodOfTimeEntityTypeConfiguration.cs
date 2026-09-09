using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class PeriodOfTimeEntityTypeConfiguration : EntityTypeConfiguration<PeriodOfTime>
{
    public override void Configure(EntityTypeBuilder<PeriodOfTime> builder)
    {
        builder.ToTable(nameof(PeriodOfTime).ToSnakeCase());

        builder.Property(e => e.End).AddLocalDateTimeOffsetConversion();
        builder.Property(e => e.Start).AddLocalDateTimeOffsetConversion();
    }
}