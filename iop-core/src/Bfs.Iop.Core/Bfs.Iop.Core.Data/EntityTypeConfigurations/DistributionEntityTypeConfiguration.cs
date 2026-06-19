using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DistributionEntityTypeConfiguration : EntityTypeConfiguration<Distribution>
{
    public override void Configure(EntityTypeBuilder<Distribution> builder)
    {
        builder.ToTable(nameof(Distribution).ToSnakeCase());

        builder.OwnsOne(e => e.Description);

        builder.OwnsOne(e => e.Title);

        builder.Property(e => e.Modified).AddLocalDateTimeOffsetConversion();
        builder.Property(e => e.Issued).AddLocalDateTimeOffsetConversion();

        builder.HasMany(c => c.AccessUrl).WithOne(m => m.DistributionAccessUrl).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ConformsTo)
            .WithOne(m => m.DistributionConformsTo)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Coverage).WithOne(c => c.DistributionCoverage).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Image).WithOne(m => m.DistributionImage).IsRequired(false)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documentation).WithOne(m => m.DistributionDocumentation).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.DownloadUrl).WithOne(m => m.DistributionDownloadUrl).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Checksum).WithOne(x => x.DistributionChecksum).HasForeignKey<CheckSum>(x => x.DistributionChecksumId).IsRequired(false)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.AccessServices)
            .WithOne(x => x.Distribution)
            .HasForeignKey(x => x.DistributionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}