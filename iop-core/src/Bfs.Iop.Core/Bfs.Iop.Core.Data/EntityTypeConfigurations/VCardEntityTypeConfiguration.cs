using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class VCardEntityTypeConfiguration : EntityTypeConfiguration<VCard>
{
    public override void Configure(EntityTypeBuilder<VCard> builder)
    {
        builder.ToTable(nameof(VCard).ToSnakeCase());

        builder.OwnsOne(e => e.AdrWork);

        builder.OwnsOne(e => e.Fn);

        builder.OwnsOne(e => e.Note);

        builder.OwnsOne(e => e.Org);
    }
}