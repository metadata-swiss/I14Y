using Bfs.Iop.DataAccess.Relational.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class ChannelOwnedByEntityTypeConfiguration : EntityTypeConfiguration<ChannelOwnedBy>
{
    public override void Configure(EntityTypeBuilder<ChannelOwnedBy> builder)
    {
        builder.ToTable("channel_owned_bys");

        builder.HasIndex(i => new { i.ChannelId, i.OwnedById }).IsUnique();
    }
}