using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class CheckSumTypeConfiguration : EntityTypeConfiguration<CheckSum>
{
    public override void Configure(EntityTypeBuilder<CheckSum> builder)
    {
        builder.ToTable(nameof(CheckSum).ToSnakeCase());
    }
}