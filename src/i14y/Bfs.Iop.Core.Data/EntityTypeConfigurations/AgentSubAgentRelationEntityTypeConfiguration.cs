using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class AgentSubAgentRelationEntityTypeConfiguration : EntityTypeConfiguration<AgentSubAgentRelation>
{
    public override void Configure(EntityTypeBuilder<AgentSubAgentRelation> builder)
    {
        builder.ToTable(nameof(AgentSubAgentRelation).ToSnakeCase());

        builder.HasIndex(x => new { x.AgentId, x.SubAgentId })
            .IsUnique();
    }
}
