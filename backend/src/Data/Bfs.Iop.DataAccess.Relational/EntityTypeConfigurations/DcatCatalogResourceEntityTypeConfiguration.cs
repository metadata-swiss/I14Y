using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DcatCatalogResourceEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalogResource>
{
    public override void Configure(EntityTypeBuilder<DcatCatalogResource> builder)
    {
        builder.ToTable(nameof(DcatCatalogResource).ToSnakeCase());
    }
}