using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Mappings;

internal static class CheckSumMappingExtensions
{
    public static ChecksumModel MapToChecksumModel(
        this CheckSum entity, 
        ChecksumAlgorithmsVocabulary checksumAlgorithmsVocabulary)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(checksumAlgorithmsVocabulary, nameof(checksumAlgorithmsVocabulary));

        return new()
        {
            Algorithm = entity.Algorithm!.MapToVocabularyEntryModel(checksumAlgorithmsVocabulary),
            ChecksumValue = entity.CheckSumValue
        };
    }

    public static CheckSum MapToCheckSum(this ChecksumInputModel inputModel, CheckSum? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.Algorithm = inputModel.Algorithm.Code;
        entity.CheckSumValue = inputModel.ChecksumValue;

        return entity;
    }
}
