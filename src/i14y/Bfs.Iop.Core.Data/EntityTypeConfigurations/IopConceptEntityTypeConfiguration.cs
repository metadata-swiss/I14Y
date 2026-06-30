using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class IopConceptEntityTypeConfiguration : EntityTypeConfiguration<IopConcept>
{
    public override void Configure(EntityTypeBuilder<IopConcept> builder)
    {
        const string tableName = "IopConcepts";

        builder.ToTable(tableName.ToSnakeCase());

        builder
            .HasMany(c => c.CodeListEntries)
            .WithOne(cl => cl.IopConcept)
            .HasForeignKey(cl => cl.IopConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.ConceptType)
            .IsRequired(true);

        builder
            .HasMany(c => c.ConformsTo)
            .WithOne(r => r.IopConceptConformsTo)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(c => c.Replaces)
            .WithOne(r => r.IopConceptReplaces)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(c => c.Description);

        builder
            .HasMany(c => c.Keywords)
            .WithOne(k => k.IopConcept)
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

        builder.Property(c => c.ValidFrom)
            .IsRequired(false);

        builder.Property(c => c.Version)
            .IsRequired(true);
    }
}
