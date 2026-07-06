using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DistributionDataServiceRelationEntityTypeConfiguration : EntityTypeConfiguration<DistributionDataServiceRelation>
{
    public override void Configure(EntityTypeBuilder<DistributionDataServiceRelation> builder)
    {
        const string tableName = "DistributionDataServiceRelations";

        builder.ToTable(tableName.ToSnakeCase());
    }
}
