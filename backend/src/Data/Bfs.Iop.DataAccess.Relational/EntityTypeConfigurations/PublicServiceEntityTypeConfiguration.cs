using Bfs.Iop.DataAccess.Relational.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class PublicServiceEntityTypeConfiguration : EntityTypeConfiguration<PublicService>
{
    public override void Configure(EntityTypeBuilder<PublicService> builder)
    {
        builder.OwnsOne(e => e.Description);

        builder.OwnsOne(e => e.Title);

        builder.HasOne(c => c.Publisher).WithMany().IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.Keyword).WithOne(c => c.PublicService).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(c => c.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(c => c.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder
            .HasOne(c => c.ResponsibleDeputy)
            .WithMany()
            .HasForeignKey(c => c.ResponsibleDeputyId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}