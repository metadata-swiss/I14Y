using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class QualifiedAttributionEntityTypeConfiguration : EntityTypeConfiguration<QualifiedAttribution>
{
    public override void Configure(EntityTypeBuilder<QualifiedAttribution> builder)
    {
        builder.ToTable(nameof(QualifiedAttribution).ToSnakeCase());

        builder.HasOne(x => x.Agent)
            .WithMany()
            .HasForeignKey(c => c.AgentId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);
    }
}