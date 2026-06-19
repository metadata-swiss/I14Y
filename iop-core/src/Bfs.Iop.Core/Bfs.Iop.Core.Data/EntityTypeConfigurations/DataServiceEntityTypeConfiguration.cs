using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bfs.Iop.Core.Data.Extensions;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DataServiceEntityTypeConfiguration : EntityTypeConfiguration<DataService>
{
    public override void Configure(EntityTypeBuilder<DataService> builder)
    {
        builder.ToTable(nameof(DataService).ToSnakeCase());

        builder.OwnsOne(e => e.Description);

        builder.OwnsOne(e => e.Title);

        builder.OwnsOne(e => e.VersionNotes);

        builder.Property(e => e.Modified).AddLocalDateTimeOffsetConversion();
        builder.Property(e => e.Issued).AddLocalDateTimeOffsetConversion();

        builder.HasMany(c => c.ContactPoint).WithOne(c => c.DataServiceContactPoint).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ConformsTo).WithOne(c => c.DataServiceConformsTo).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documentation).WithOne(c => c.DataServiceDocumentation).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.EndpointDescription).WithOne(c => c.DataServiceEndpointDescription).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.EndpointUrl).WithOne(c => c.DataServiceEndpointUrl).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.EndpointUrl).WithOne(c => c.DataServiceEndpointUrl).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Keyword).WithOne(c => c.DataService).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LandingPage).WithOne(m => m.DataServiceLandingPage).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LandingPage).WithOne(m => m.DataServiceLandingPage).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.PreviousVersion).WithMany().IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Publisher).WithMany().IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);

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