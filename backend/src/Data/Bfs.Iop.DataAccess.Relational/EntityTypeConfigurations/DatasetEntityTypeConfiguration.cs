using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DatasetEntityTypeConfiguration : EntityTypeConfiguration<Dataset>
{
    public override void Configure(EntityTypeBuilder<Dataset> builder)
    {
        builder.ToTable(nameof(Dataset).ToSnakeCase());

        builder.OwnsOne(e => e.Description);

        builder.OwnsOne(e => e.Title);

        builder.OwnsOne(e => e.VersionNotes);

        builder.OwnsOne(e => e.QualifiedAttributionComplement);

        builder.OwnsOne(e => e.RetentionPeriodDescription);

        builder.Property(e => e.Modified).AddLocalDateTimeOffsetConversion();
        builder.Property(e => e.Issued).AddLocalDateTimeOffsetConversion();

        builder.HasMany(c => c.ContactPoint).WithOne(c => c.DatasetContactPoint).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ConformsTo).WithOne(m => m.DataSetConformsTo).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Distributions).WithOne(d => d.Dataset).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documentation).WithOne(m => m.DatasetDocumentation).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Image).WithOne(m => m.DataSetImage).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.IsReferencedBy).WithOne(m => m.DataSetIsReferencedBy).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Keyword).WithOne(c => c.Dataset).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LandingPage).WithOne(m => m.DatasetLandingPage).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.PreviousVersion).WithMany().IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Publisher).WithMany().IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.QualifiedAttribution).WithOne(c => c.Dataset).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.QualifiedRelation).WithOne(c => c.Dataset).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Relation).WithOne(m => m.DataSetRelation).IsRequired(false)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.TemporalCoverage).WithOne(c => c.TemporalCoverage).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Modified).HasColumnName("modified");

        builder
            .HasOne(c => c.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(c => c.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder
            .HasOne(c => c.ResponsibleDeputy)
            .WithMany()
            .HasForeignKey(c => c.ResponsibleDeputyId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}