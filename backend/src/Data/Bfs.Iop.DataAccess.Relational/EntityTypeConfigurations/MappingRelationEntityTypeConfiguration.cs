using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class MappingRelationEntityTypeConfiguration : EntityTypeConfiguration<MappingRelation>
{
    public override void Configure(EntityTypeBuilder<MappingRelation> builder)
    {
        builder.ToTable(("MappingRelations").ToSnakeCase());

        builder.Property(r => r.SourceCodeUri).IsRequired(true);
        builder.Property(r => r.TargetCodeUri).IsRequired(true);
        builder.Property(r => r.RelationType).IsRequired(true);

        builder
            .HasOne(r => r.MappingTable)
            .WithMany()
            .HasForeignKey(r => r.MappingTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(r => new { r.MappingTableId, r.SourceCodeUri, r.TargetCodeUri })
            .IsUnique(true);
    }
}
