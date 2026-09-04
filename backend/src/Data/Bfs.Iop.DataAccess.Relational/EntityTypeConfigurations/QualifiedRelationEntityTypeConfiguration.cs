using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class QualifiedRelationEntityTypeConfiguration : EntityTypeConfiguration<QualifiedRelation>
{
    public override void Configure(EntityTypeBuilder<QualifiedRelation> builder) =>
        builder.ToTable(nameof(QualifiedRelation).ToSnakeCase());
}