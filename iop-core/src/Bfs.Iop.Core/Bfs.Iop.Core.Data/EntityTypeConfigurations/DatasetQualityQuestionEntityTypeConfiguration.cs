using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DatasetQualityQuestionEntityTypeConfiguration : EntityTypeConfiguration<DatasetQualityQuestion>
{
    public override void Configure(EntityTypeBuilder<DatasetQualityQuestion> builder)
    {
        builder.ToTable(nameof(DatasetQualityQuestion).ToSnakeCase());

        builder.OwnsOne(e => e.Question);

        builder.HasMany(c => c.AnswerOptions).WithOne(m => m.Question).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.QualityInformations).WithOne(m => m.Question).IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}