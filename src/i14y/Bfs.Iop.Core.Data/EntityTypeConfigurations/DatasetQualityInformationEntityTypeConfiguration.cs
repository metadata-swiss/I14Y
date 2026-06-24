using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal class DatasetQualityInformationEntityTypeConfiguration : EntityTypeConfiguration<DatasetQualityInformation>
{
    public override void Configure(EntityTypeBuilder<DatasetQualityInformation> builder)
    {
        builder.ToTable(nameof(DatasetQualityInformation).ToSnakeCase());

        builder.HasOne(e => e.Dataset).WithMany().IsRequired(true).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Answer).WithMany(x => x.QualityInformations).HasForeignKey(x => x.AnswerId).IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Question).WithMany(x => x.QualityInformations).HasForeignKey(x => x.QuestionId).IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}