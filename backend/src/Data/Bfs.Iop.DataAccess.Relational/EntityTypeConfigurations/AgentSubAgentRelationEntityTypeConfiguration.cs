using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class AgentSubAgentRelationEntityTypeConfiguration : EntityTypeConfiguration<AgentSubAgentRelation>
{
    public override void Configure(EntityTypeBuilder<AgentSubAgentRelation> builder)
    {
        builder.ToTable(nameof(AgentSubAgentRelation).ToSnakeCase());

        builder.HasIndex(x => new { x.AgentId, x.SubAgentId })
            .IsUnique();
    }
}
