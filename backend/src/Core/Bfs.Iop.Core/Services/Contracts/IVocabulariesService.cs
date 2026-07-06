using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Services.Contracts;

internal interface IVocabulariesService
{
    Task<T?> TryGetVocabulary<T>(CancellationToken cancellationToken = default) where T : IdentifiedVocabularyBase, new();

    Task<VocabularyModel?> TryGetVocabulary(string vocabularyIdentifier, CancellationToken cancellationToken);

    Task<VocabularyModel> GetVocabulary(
        string vocabularyIdentifier,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VocabularyConfigModel>> GetVocabularyConfigs(CancellationToken cancellationToken = default);

    Task<VocabularyConfigModel> GetVocabularyConfig(Guid id, CancellationToken cancellationToken = default);

    Task UpdateVocabularyConfig(Guid id, VocabularyConfigInputModel model, CancellationToken cancellationToken = default);

    Task<Guid> AddVocabularyConfig(VocabularyConfigInputModel model, CancellationToken cancellationToken = default);

    Task DeleteVocabularyConfig(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that all vocabularies are loaded into the service cache and ready to be used.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BuildAllVocabulariesInCache(CancellationToken cancellationToken = default);
}
