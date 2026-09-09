using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class ResourceEntityTypeConfiguration : EntityTypeConfiguration<Resource>
{
    public override void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable(nameof(Resource).ToSnakeCase());

        builder.OwnsOne(e => e.Label);

        builder.HasOne(p => p.QualifiedRelation).WithOne(p => p.Relation).OnDelete(DeleteBehavior.Cascade);
    }
}