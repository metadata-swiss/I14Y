using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Mappings;

internal static class PeriodOfTimeMappingExtensions
{
    public static PeriodOfTimeModel MapToDateOnlyPeriodOfTimeModel(this PeriodOfTime entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            End = entity.End,
            Start = entity.Start,
        };
    }

    public static PeriodOfTime MapToPeriodOfTime(
        this PeriodOfTimeModel model, 
        PeriodOfTime? entity = null)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        entity ??= new();

        entity.End = model.End;
        entity.Start = model.Start;

        return entity;
    }

    public static IEnumerable<PeriodOfTime> MapToPeriodsOfTime(
       this IEnumerable<PeriodOfTimeModel> models,
       ICollection<PeriodOfTime> entities)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return models.Select(
            (p, i) => p.MapToPeriodOfTime(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
