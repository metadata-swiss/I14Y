using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class AnnotationEntityTypeConfiguration : EntityTypeConfiguration<Annotation>
{
    public override void Configure(EntityTypeBuilder<Annotation> builder)
    {
        const string tableName = "Annotations";

        builder.ToTable(tableName.ToSnakeCase());

        builder.Property(a => a.Position).IsRequired();

        builder.OwnsOne(a => a.Text);

        builder.Property(a => a.Type).IsRequired();
    }
}
