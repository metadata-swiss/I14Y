using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class VocabularyConfigEntityTypeConfiguration : EntityTypeConfiguration<VocabularyConfig>
{
    public override void Configure(EntityTypeBuilder<VocabularyConfig> builder)
    {
        builder.ToTable(nameof(VocabularyConfig).ToSnakeCase());

        builder.HasIndex(x => x.VocabularyIdentifier)
               .IsUnique();
    }
}