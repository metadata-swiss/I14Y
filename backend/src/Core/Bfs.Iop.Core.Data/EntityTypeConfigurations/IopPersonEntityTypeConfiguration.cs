using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class IopPersonEntityTypeConfiguration : EntityTypeConfiguration<IopPerson>
{
    public override void Configure(EntityTypeBuilder<IopPerson> builder)
    {
        builder
            .HasIndex(p => p.Email)
            .IsUnique();
    }
}