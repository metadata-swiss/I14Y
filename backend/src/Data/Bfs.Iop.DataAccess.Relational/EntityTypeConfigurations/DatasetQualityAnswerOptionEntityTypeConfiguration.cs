using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal class DatasetQualityAnswerOptionEntityTypeConfiguration : EntityTypeConfiguration<DatasetQualityAnswerOption>
{
    public override void Configure(EntityTypeBuilder<DatasetQualityAnswerOption> builder)
    {
        builder.ToTable(nameof(DatasetQualityAnswerOption).ToSnakeCase());

        builder.OwnsOne(e => e.Detail);

        builder.OwnsOne(e => e.Name);

        builder.HasMany(c => c.QualityInformations).WithOne(m => m.Answer).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Question).WithMany(x => x.AnswerOptions).HasForeignKey(x => x.DatasetQualityQuestionId).IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}