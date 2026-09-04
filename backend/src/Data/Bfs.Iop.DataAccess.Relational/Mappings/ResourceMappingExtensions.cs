using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class ResourceMappingExtensions
{
    public static ResourceModel MapToResourceModel(this Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource, nameof(resource));

        return new()
        {
            Label = resource.Label?.MapToMultiLanguageModel(),
            Uri = resource.Href
        };
    }

    public static Resource MapToResource(this ResourceModel resourceModel, Resource? entity = null)
    {
        ArgumentNullException.ThrowIfNull(resourceModel, nameof(resourceModel));

        entity ??= new();

        entity.Label = resourceModel.Label?.MapToMultiLanguage();
        entity.Href = resourceModel.Uri;

        return entity;
    }

    public static IEnumerable<Resource> MapToResources(
        this IEnumerable<ResourceModel> inputModels,
        ICollection<Resource> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (r, i) => r.MapToResource(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
