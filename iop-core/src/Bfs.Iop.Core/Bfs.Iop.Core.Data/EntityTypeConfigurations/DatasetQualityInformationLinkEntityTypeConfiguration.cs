using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DatasetQualityInformationLinkEntityTypeConfiguration : EntityTypeConfiguration<DatasetQualityInformationLink>
{
    public override void Configure(EntityTypeBuilder<DatasetQualityInformationLink> builder)
    {
        builder.ToTable(nameof(DatasetQualityInformationLink).ToSnakeCase());

        builder.HasOne(e => e.Dataset).WithMany().IsRequired(true).OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.Label);
    }
}