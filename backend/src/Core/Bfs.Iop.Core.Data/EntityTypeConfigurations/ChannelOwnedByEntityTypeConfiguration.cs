using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class ChannelOwnedByEntityTypeConfiguration : EntityTypeConfiguration<ChannelOwnedBy>
{
    public override void Configure(EntityTypeBuilder<ChannelOwnedBy> builder)
    {
        builder.ToTable("channel_owned_bys");

        builder.HasIndex(i => new { i.ChannelId, i.OwnedById }).IsUnique();
    }
}