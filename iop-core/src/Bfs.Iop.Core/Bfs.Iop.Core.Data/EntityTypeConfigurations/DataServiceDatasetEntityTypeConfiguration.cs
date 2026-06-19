using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DataServiceDatasetEntityTypeConfiguration : EntityTypeConfiguration<DataServiceDataset>
{
    public override void Configure(EntityTypeBuilder<DataServiceDataset> builder)
    {
        builder.ToTable(nameof(DataServiceDataset).ToSnakeCase());

        builder.HasIndex(i => new { i.DataServiceId, i.DatasetId }).IsUnique();
    }
}