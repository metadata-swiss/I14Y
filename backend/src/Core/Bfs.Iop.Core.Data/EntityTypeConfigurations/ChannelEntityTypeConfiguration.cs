using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class ChannelEntityTypeConfiguration : EntityTypeConfiguration<Channel>
{
    public override void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.OwnsOne(e => e.Address);
        builder.OwnsOne(e => e.Description);
        builder.Property(x => x.Identifier).IsRequired();
    }
}