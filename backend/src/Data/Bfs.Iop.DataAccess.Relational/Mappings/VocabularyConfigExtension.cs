using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class VocabularyConfigExtension
{
    public static VocabularyConfigModel MapToVocabularyConfigModel(this VocabularyConfig vocabularyConfig)
    {
        ArgumentNullException.ThrowIfNull(vocabularyConfig, nameof(vocabularyConfig));

        return new()
        {
            Id = vocabularyConfig.Id,
            ConceptIdentifier = vocabularyConfig.ConceptIdentifier,
            VocabularyIdentifier = vocabularyConfig.VocabularyIdentifier,
            ConceptVersion = vocabularyConfig.ConceptVersion
        };
    }

    public static VocabularyConfig MapToVocabularyConfig(
        this VocabularyConfigInputModel vocabularyConfigInput, 
        VocabularyConfig? entity = null)
    {
        ArgumentNullException.ThrowIfNull(vocabularyConfigInput, nameof(vocabularyConfigInput));

        entity ??= new();

        entity.ConceptIdentifier = vocabularyConfigInput.ConceptIdentifier;
        entity.VocabularyIdentifier = vocabularyConfigInput.VocabularyIdentifier;
        entity.ConceptVersion = vocabularyConfigInput.ConceptVersion;

        return entity;
    }
}
