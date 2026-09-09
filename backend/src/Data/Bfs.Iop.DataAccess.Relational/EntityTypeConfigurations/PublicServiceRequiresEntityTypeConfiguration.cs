using Bfs.Iop.DataAccess.Relational.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class PublicServiceRequiresEntityTypeConfiguration : EntityTypeConfiguration<PublicServiceRequires>
{
    public override void Configure(EntityTypeBuilder<PublicServiceRequires> builder)
    {
        builder.HasIndex(i => new { i.PublicServiceId, i.RequiresId }).IsUnique();
        builder.HasOne(x => x.PublicService).WithMany(x => x.Requires);
    }
}