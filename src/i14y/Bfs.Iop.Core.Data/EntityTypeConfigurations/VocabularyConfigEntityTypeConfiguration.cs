using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class VocabularyConfigEntityTypeConfiguration : EntityTypeConfiguration<VocabularyConfig>
{
    public override void Configure(EntityTypeBuilder<VocabularyConfig> builder)
    {
        builder.ToTable(nameof(VocabularyConfig).ToSnakeCase());

        builder.HasIndex(x => x.VocabularyIdentifier)
               .IsUnique();
    }
}