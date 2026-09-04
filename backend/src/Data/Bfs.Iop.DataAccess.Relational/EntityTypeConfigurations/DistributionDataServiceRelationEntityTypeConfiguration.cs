using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DistributionDataServiceRelationEntityTypeConfiguration : EntityTypeConfiguration<DistributionDataServiceRelation>
{
    public override void Configure(EntityTypeBuilder<DistributionDataServiceRelation> builder)
    {
        const string tableName = "DistributionDataServiceRelations";

        builder.ToTable(tableName.ToSnakeCase());
    }
}
