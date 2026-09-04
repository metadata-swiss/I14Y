using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

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