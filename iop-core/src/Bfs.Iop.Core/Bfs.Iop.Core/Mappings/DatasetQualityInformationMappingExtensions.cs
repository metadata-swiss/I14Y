using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Mappings;

internal static class DatasetQualityInformationMappingExtensions
{
    public static DatasetQualityInformationLinkModel MapToDatasetQualityInformationLinkModel(this DatasetQualityInformationLink entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Href = entity.Href,
            Id = entity.Id,
            Label = entity.Label.MapToMultiLanguageModel()
        };
    }

    public static DatasetQualityInformationLink MapToDatasetQualityInformationLink(
        this DatasetQualityInformationLinkModel model, 
        Guid datasetId, 
        DatasetQualityInformationLink? entity = null)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        entity ??= new();

        entity.DatasetId = datasetId;
        entity.Href = model.Href;
        entity.Label = model.Label?.MapToMultiLanguage();

        return entity;
    }

    public static DatasetQualityInformationModel MapToDatasetQualityInformationModel(this DatasetQualityInformation entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            AnswerId = entity.AnswerId,
            Detail = entity.Detail,
            Id = entity.Id,
            QuestionId = entity.QuestionId,
        };
    }

    public static DatasetQualityInformation MapToDatasetQualityInformation(
        this DatasetQualityInformationModel model,
        Guid datasetId,
        DatasetQualityInformation? entity = null)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        entity ??= new();

        entity.AnswerId = model.AnswerId;
        entity.DatasetId = datasetId;
        entity.Detail = model.Detail;
        entity.QuestionId = model.QuestionId;

        return entity;
    }
}
