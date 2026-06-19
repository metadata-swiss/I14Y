using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class PublicServiceRelationEntityTypeConfiguration : EntityTypeConfiguration<PublicServiceRelation>
{
    public override void Configure(EntityTypeBuilder<PublicServiceRelation> builder)
    {
        builder.ToTable("public_service_relations");

        builder.HasIndex(i => new { i.PublicServiceId, i.RelationId }).IsUnique();
        builder.HasOne(x => x.PublicService).WithMany(x => x.Relation);
    }
}