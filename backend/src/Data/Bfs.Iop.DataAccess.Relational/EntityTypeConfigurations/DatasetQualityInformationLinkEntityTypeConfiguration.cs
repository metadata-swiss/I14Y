using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DatasetQualityInformationLinkEntityTypeConfiguration : EntityTypeConfiguration<DatasetQualityInformationLink>
{
    public override void Configure(EntityTypeBuilder<DatasetQualityInformationLink> builder)
    {
        builder.ToTable(nameof(DatasetQualityInformationLink).ToSnakeCase());

        builder.HasOne(e => e.Dataset).WithMany().IsRequired(true).OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.Label);
    }
}