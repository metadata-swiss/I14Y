using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Mappings;

internal static class CodeListEntryMappingExtensions
{
    public static CodeListEntryModel MapToCodeListEntryModel(
        this CodeListEntry codeListEntry)
    {
        ArgumentNullException.ThrowIfNull(codeListEntry, nameof(codeListEntry));

        return new()
        {
            Annotations = codeListEntry.Annotations.Select(a => a.MapToAnnotationModel()),
            Code = codeListEntry.Code,
            ConceptId = codeListEntry.IopConceptId,
            Description = codeListEntry.Description?.MapToMultiLanguageModel(),
            Id = codeListEntry.Id,
            Name = codeListEntry.Name.MapToMultiLanguageModel(),
            ParentCode = codeListEntry.ParentCodeListEntry?.Code,
            ValidFrom = codeListEntry.ValidFrom,
            ValidTo = codeListEntry.ValidTo,
        };
    }

    public static CodeListEntry MapToCodeListEntry(
        this CodeListEntryInputModel inputModel,
        Guid conceptId,
        int position,
        Guid? parentCodeListEntryId = null,
        CodeListEntry? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 0, nameof(position));

        entity ??= new()
        {
            Id = Guid.NewGuid()
        };

        entity.Annotations = inputModel.Annotations.MapToAnnotations(entity.Id, entity.Annotations).ToList();
        entity.Code = inputModel.Code;
        entity.Description = inputModel.Description?.MapToMultiLanguage();
        entity.IopConceptId = conceptId;
        entity.Name = inputModel.Name.MapToMultiLanguage();
        entity.ParentCodeListEntryId = parentCodeListEntryId;
        entity.Position = position;
        entity.ValidFrom = inputModel.ValidFrom;
        entity.ValidTo = inputModel.ValidTo;

        return entity;
    }

    public static CodeListEntryInputModel MapToCodeListEntryInputModel(
        this CodeListEntryModel codeListEntry)
    {
        ArgumentNullException.ThrowIfNull(codeListEntry, nameof(codeListEntry));

        return new()
        {
            Annotations = codeListEntry.Annotations.Select(a => a.MapToAnnotationInputModel()),
            Code = codeListEntry.Code,
            Description = codeListEntry.Description,
            Name = codeListEntry.Name,
            ParentCode = codeListEntry.ParentCode,
            ValidFrom = codeListEntry.ValidFrom,
            ValidTo = codeListEntry.ValidTo,
        };
    }

    public static VocabularyEntryModel MapToVocabularyEntryModel(this CodeListEntry entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Code = entity.Code,
            Name = entity.Name.MapToMultiLanguageModel(),
            Uri = TryGetUri(entity.Annotations)
        };
    }

    private static string? TryGetUri(IEnumerable<Annotation> annotations)
    {
        const string type = "EXT_RESOURCE";
        var annotation = annotations.FirstOrDefault(x => x.Type == type);
        return annotation?.Uri;
    }
}