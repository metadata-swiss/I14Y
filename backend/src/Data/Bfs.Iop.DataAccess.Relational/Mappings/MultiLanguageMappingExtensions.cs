using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class MultiLanguageMappingExtensions
{
    public static MultiLanguageModel MapToMultiLanguageModel(this MultiLanguage multiLanguage)
    {
        ArgumentNullException.ThrowIfNull(multiLanguage, nameof(multiLanguage));

        return new()
        {
            De = multiLanguage.De,
            En = multiLanguage.En,
            Fr = multiLanguage.Fr,
            It = multiLanguage.It,
            Rm = multiLanguage.Rm,
        };
    }

    public static MultiLanguage MapToMultiLanguage(
        this MultiLanguageModel multiLanguageModel, 
        MultiLanguage? entity = null)
    {
        ArgumentNullException.ThrowIfNull(multiLanguageModel, nameof(multiLanguageModel));

        entity ??= new();

        entity.De = multiLanguageModel.De;
        entity.En = multiLanguageModel.En;
        entity.Fr = multiLanguageModel.Fr;
        entity.It = multiLanguageModel.It;
        entity.Rm = multiLanguageModel.Rm;

        return entity;
    }
}
