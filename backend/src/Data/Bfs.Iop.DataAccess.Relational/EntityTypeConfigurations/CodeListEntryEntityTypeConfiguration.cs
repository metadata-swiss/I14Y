using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class CodeListEntryEntityTypeConfiguration : EntityTypeConfiguration<CodeListEntry>
{
    public override void Configure(EntityTypeBuilder<CodeListEntry> builder)
    {
        const string tableName = "CodeListEntries";

        builder.ToTable(tableName.ToSnakeCase());

        builder
            .HasMany(c => c.Annotations)
            .WithOne(a => a.CodeListEntry)
            .HasForeignKey(a => a.CodeListEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Code).IsRequired(true);

        builder.OwnsOne(c => c.Description);

        builder.OwnsOne(c => c.Name);

        builder
            .HasOne(c => c.ParentCodeListEntry)
            .WithMany()
            .HasForeignKey(c => c.ParentCodeListEntryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(c => c.Position).IsRequired(true);

        builder
            .HasIndex(c => new { c.IopConceptId, c.Code })
            .IsUnique(true);
    }
}
