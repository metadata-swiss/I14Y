using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class DatasetQualityQuestionMappingExtensions
{
    public static DatasetQualityQuestionModel MapToDatasetQualityQuestionModel(this DatasetQualityQuestion entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            AnswerOptions = entity.AnswerOptions.Select(x => x.MapToDatasetQualityAnswerOption()).ToList(),
            Id = entity.Id,
            IsMandatory = entity.Mandatory,
            Order = entity.Order,
            Question = entity.Question.MapToMultiLanguageModel()
        };
    }

    private static DatasetQualityAnswerOptionModel MapToDatasetQualityAnswerOption(this DatasetQualityAnswerOption entity) => 
        new()
        {
            Detail = entity.Detail?.MapToMultiLanguageModel(),
            Id = entity.Id,
            IsDetailMandatory = entity.DetailMandatory,
            Name = entity.Name.MapToMultiLanguageModel(),
            Value = entity.Value,
        };
}
