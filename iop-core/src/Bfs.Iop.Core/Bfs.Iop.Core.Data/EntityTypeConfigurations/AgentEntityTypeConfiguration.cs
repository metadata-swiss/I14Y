using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class AgentEntityTypeConfiguration : EntityTypeConfiguration<Agent>
{
    public override void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable(nameof(Agent).ToSnakeCase());

        builder.OwnsOne(e => e.Name);

        builder.OwnsOne(x => x.Description);

        builder.OwnsOne(x => x.PrefLabel);

        builder.HasOne(x => x.ContactPoint)
            .WithOne(x => x.AgentContactPoint)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne(m => m.AgentImage)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SubAgents)
            .WithOne(x => x.Agent)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Identifier).IsUnique(true);
    }
}