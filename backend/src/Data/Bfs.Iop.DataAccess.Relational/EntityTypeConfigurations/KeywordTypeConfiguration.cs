using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class KeywordTypeConfiguration : EntityTypeConfiguration<Keyword>
{
    public override void Configure(EntityTypeBuilder<Keyword> builder)
    {
        builder.ToTable(nameof(Keyword).ToSnakeCase());

        builder.OwnsOne(e => e.Text);
    }
}