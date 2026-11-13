using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DcatCatalogEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalog>
{
    public override void Configure(EntityTypeBuilder<DcatCatalog> builder)
    {
        builder.ToTable(nameof(DcatCatalog).ToSnakeCase());

        builder.OwnsOne(e => e.Title);
        builder.OwnsOne(e => e.Description);

        builder.OwnsOne(e => e.ExternalPublisherName);

        builder.HasOne(c => c.Publisher).WithMany().IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Tenant).WithMany().IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);
    }
}
