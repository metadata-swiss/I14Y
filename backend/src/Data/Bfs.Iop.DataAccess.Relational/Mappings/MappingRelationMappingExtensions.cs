using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class MappingRelationMappingExtensions
{
    public static MappingRelationModel MapToMappingRelationModel(
        this MappingRelation entity,
        IVocabulariesService vocabulariesService,
        CodeListEntry? sourceCodeListEntry = null,
        CodeListEntry? targetCodeListEntry = null)
    { 
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var relationsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<MappingPredicateVocabulary>();

        return new()
        {
            Id = entity.Id,
            RelationType = entity.RelationType.MapToVocabularyEntryModel(relationsVocabulary),
            Source = new()
            {
                Code = sourceCodeListEntry?.Code,
                Name = sourceCodeListEntry?.Name.MapToMultiLanguageModel(),
                Uri = entity.SourceCodeUri
            },
            Target = new()
            {
                Code = targetCodeListEntry?.Code,
                Name = targetCodeListEntry?.Name.MapToMultiLanguageModel(),
                Uri = entity.TargetCodeUri
            }
        };
    }

    public static MappingRelation MapToMappingRelation(
        this MappingRelationInputModel inputModel,
        Guid mappingTableId,
        MappingRelation? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new()
        {
            Id = Guid.NewGuid()
        };

        entity.MappingTableId = mappingTableId;
        entity.SourceCodeUri = inputModel.Source.Uri;
        entity.TargetCodeUri = inputModel.Target.Uri;
        entity.RelationType = inputModel.RelationType.Code;

        return entity;
    }

    public static MappingRelationInputModel MapToMappingRelationInputModel(this MappingRelationModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new()
        {
            RelationType = new() { Code = model.RelationType.Code },
            Source = new() { Uri = model.Source.Uri },
            Target = new() { Uri = model.Target.Uri }
        };
    }
}
