using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class PublicServiceRequiresEntityTypeConfiguration : EntityTypeConfiguration<PublicServiceRequires>
{
    public override void Configure(EntityTypeBuilder<PublicServiceRequires> builder)
    {
        builder.HasIndex(i => new { i.PublicServiceId, i.RequiresId }).IsUnique();
        builder.HasOne(x => x.PublicService).WithMany(x => x.Requires);
    }
}