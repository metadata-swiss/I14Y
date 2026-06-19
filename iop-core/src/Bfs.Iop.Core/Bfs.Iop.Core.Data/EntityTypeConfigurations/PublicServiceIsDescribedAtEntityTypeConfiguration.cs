using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class PublicServiceIsDescribedAtEntityTypeConfiguration : EntityTypeConfiguration<PublicServiceIsDescribedAt>
{
    public override void Configure(EntityTypeBuilder<PublicServiceIsDescribedAt> builder)
    {
        builder.ToTable("public_service_is_described_ats");

        builder.HasIndex(i => new { i.PublicServiceId, i.IsDescribedAtId }).IsUnique();
        builder.HasOne(x => x.PublicService).WithMany(x => x.IsDescribedAt);
    }
}