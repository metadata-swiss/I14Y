using Bfs.Iop.DataAccess.Relational.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class IopPersonEntityTypeConfiguration : EntityTypeConfiguration<IopPerson>
{
    public override void Configure(EntityTypeBuilder<IopPerson> builder)
    {
        builder
            .HasIndex(p => p.Email)
            .IsUnique();
    }
}