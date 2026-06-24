using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DcatCatalogThemeEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalogTheme>
{
    public override void Configure(EntityTypeBuilder<DcatCatalogTheme> builder)
    {
        builder.ToTable(nameof(DcatCatalogTheme).ToSnakeCase());
    }
}