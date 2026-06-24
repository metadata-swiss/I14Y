using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class QualifiedRelationEntityTypeConfiguration : EntityTypeConfiguration<QualifiedRelation>
{
    public override void Configure(EntityTypeBuilder<QualifiedRelation> builder) =>
        builder.ToTable(nameof(QualifiedRelation).ToSnakeCase());
}