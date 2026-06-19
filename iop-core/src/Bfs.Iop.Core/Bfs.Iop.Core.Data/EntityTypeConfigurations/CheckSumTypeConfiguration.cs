using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class CheckSumTypeConfiguration : EntityTypeConfiguration<CheckSum>
{
    public override void Configure(EntityTypeBuilder<CheckSum> builder)
    {
        builder.ToTable(nameof(CheckSum).ToSnakeCase());
    }
}