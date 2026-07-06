using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class KeywordTypeConfiguration : EntityTypeConfiguration<Keyword>
{
    public override void Configure(EntityTypeBuilder<Keyword> builder)
    {
        builder.ToTable(nameof(Keyword).ToSnakeCase());

        builder.OwnsOne(e => e.Text);
    }
}