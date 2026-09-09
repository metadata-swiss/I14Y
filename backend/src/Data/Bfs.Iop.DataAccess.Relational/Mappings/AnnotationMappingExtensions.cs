using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class AnnotationMappingExtensions
{
    public static AnnotationModel MapToAnnotationModel(this Annotation annotation)
    {
        ArgumentNullException.ThrowIfNull(annotation, nameof(annotation));

        return new()
        {
            Id = annotation.Id,
            Identifier = annotation.Identifier,
            Text = annotation.Text?.MapToMultiLanguageModel(),
            Title = annotation.Title,
            Type = annotation.Type,
            Uri = annotation.Uri,
            CodeListEntryId = annotation.CodeListEntryId,
        };
    }

    public static Annotation MapToAnnotation(
        this AnnotationInputModel inputModel,
        Guid codeListEntryId, 
        int position,
        Annotation? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 0, nameof(position));

        entity ??= new();

        entity.CodeListEntryId = codeListEntryId;
        entity.Identifier = inputModel.Identifier;
        entity.Position = position;
        entity.Text = inputModel.Text?.MapToMultiLanguage();
        entity.Title = inputModel.Title;
        entity.Type = inputModel.Type;
        entity.Uri = inputModel.Uri;

        return entity;
    }

    public static IEnumerable<Annotation> MapToAnnotations(
        this IEnumerable<AnnotationInputModel> inputModels,
        Guid codeListEntryId,
        ICollection<Annotation> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (a, i) => a.MapToAnnotation(
                codeListEntryId, 
                i, 
                entities.Count > i 
                    ? entities.ElementAt(i) 
                    : new()));
    }

    public static AnnotationInputModel MapToAnnotationInputModel(this AnnotationModel annotation)
    {
        ArgumentNullException.ThrowIfNull(annotation, nameof(annotation));

        return new()
        {
            Identifier = annotation.Identifier,
            Text = annotation.Text,
            Title = annotation.Title,
            Type = annotation.Type,
            Uri = annotation.Uri,
        };
    }
}