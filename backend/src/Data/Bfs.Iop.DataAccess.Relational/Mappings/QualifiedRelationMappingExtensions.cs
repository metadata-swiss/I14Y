using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class QualifiedRelationMappingExtensions
{
    public static DcatQualifiedRelationModel MapToQualifiedRelationModel(
        this QualifiedRelation entity,
        RelationshipRolesVocabulary relationshipRolesVocabulary)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(relationshipRolesVocabulary, nameof(relationshipRolesVocabulary));

        return new()
        {
            HadRole = entity.HadRole.MapToVocabularyEntryModel(relationshipRolesVocabulary),
            Relation = entity.Relation.MapToResourceModel()
        };
    }

    public static QualifiedRelation MapToQualifiedRelation(
        this DcatQualifiedRelationInputModel inputModel,
        QualifiedRelation? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.HadRole = inputModel.HadRole.Code;
        entity.Relation = inputModel.Relation.MapToResource(entity.Relation);

        return entity;
    }

    public static IEnumerable<QualifiedRelation> MapToQualifiedRelations(
        this IEnumerable<DcatQualifiedRelationInputModel> models,
        ICollection<QualifiedRelation> entities)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return models.Select(
            (q, i) => q.MapToQualifiedRelation(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
