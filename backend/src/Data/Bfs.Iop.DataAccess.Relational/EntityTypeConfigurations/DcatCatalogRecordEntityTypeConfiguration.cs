using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DcatCatalogRecordEntityTypeConfiguration : EntityTypeConfiguration<DcatCatalogRecord>
{
    public override void Configure(EntityTypeBuilder<DcatCatalogRecord> builder)
    {
        builder.ToTable(nameof(DcatCatalogRecord).ToSnakeCase());

        builder.HasOne(c => c.PrimaryTopic)
            .WithOne(c => c.DcatCatalogRecord)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.DcatCatalog).WithMany().HasForeignKey(x => x.DcatCatalogId).OnDelete(DeleteBehavior.Cascade);
    }
}