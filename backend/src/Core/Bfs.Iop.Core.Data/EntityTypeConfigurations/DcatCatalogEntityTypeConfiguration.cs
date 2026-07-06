using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DcatCatalogEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalog>
{
    public override void Configure(EntityTypeBuilder<DcatCatalog> builder)
    {
        builder.ToTable(nameof(DcatCatalog).ToSnakeCase());

        builder.OwnsOne(e => e.Title);
        builder.OwnsOne(e => e.Description);

        builder.HasOne(c => c.Publisher).WithMany().IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);
    }
}
