using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DcatCatalogThemeEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalogTheme>
{
    public override void Configure(EntityTypeBuilder<DcatCatalogTheme> builder)
    {
        builder.ToTable(nameof(DcatCatalogTheme).ToSnakeCase());
    }
}