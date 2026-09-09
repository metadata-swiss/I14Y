using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DataServiceDatasetEntityTypeConfiguration : EntityTypeConfiguration<DataServiceDataset>
{
    public override void Configure(EntityTypeBuilder<DataServiceDataset> builder)
    {
        builder.ToTable(nameof(DataServiceDataset).ToSnakeCase());

        builder.HasIndex(i => new { i.DataServiceId, i.DatasetId }).IsUnique();
    }
}