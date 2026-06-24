using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DcatCatalogResourceEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalogResource>
{
    public override void Configure(EntityTypeBuilder<DcatCatalogResource> builder)
    {
        builder.ToTable(nameof(DcatCatalogResource).ToSnakeCase());
    }
}