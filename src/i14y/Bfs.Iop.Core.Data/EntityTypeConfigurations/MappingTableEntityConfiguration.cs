using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class MappingTableEntityConfiguration : EntityTypeConfiguration<MappingTable>
{
    public override void Configure(EntityTypeBuilder<MappingTable> builder)
    {
        builder.ToTable(("MappingTables").ToSnakeCase());

        builder
            .HasMany(m => m.Relations)
            .WithOne(r => r.MappingTable)
            .HasForeignKey(r => r.MappingTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(c => c.ConformsTo)
            .WithOne(r => r.MappingTableConformsTo)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(c => c.Description);

        builder
            .HasMany(c => c.Keywords)
            .WithOne(k => k.MappingTable)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(c => c.Name);

        builder
            .HasOne(c => c.Publisher)
            .WithMany()
            .HasForeignKey(c => c.PublisherId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(c => c.ResponsibleDeputy)
            .WithMany()
            .HasForeignKey(c => c.ResponsibleDeputyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(c => c.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(c => c.ResponsiblePersonId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(c => c.SourceUri)
            .IsRequired(true);

        builder.Property(c => c.TargetUri)
            .IsRequired(true);

        builder.Property(c => c.Version)
            .IsRequired(true);
    }
}
