using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Services.Contracts;

/// <summary>
/// Core's full vocabulary service: the read half plus configuration CRUD.
/// <para>
/// Extends <see cref="IVocabularyReader"/> rather than redeclaring its members, so the read half has
/// one definition. The reader lives in <c>Bfs.Iop.Core.Data</c> because search resolves vocabulary
/// labels without any of the CRUD — and without the authorization stack that CRUD requires.
/// </para>
/// </summary>
internal interface IVocabulariesService : IVocabularyReader
{

    Task<VocabularyModel> GetVocabulary(
        string vocabularyIdentifier,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VocabularyConfigModel>> GetVocabularyConfigs(CancellationToken cancellationToken = default);

    Task<VocabularyConfigModel> GetVocabularyConfig(Guid id, CancellationToken cancellationToken = default);

    Task UpdateVocabularyConfig(Guid id, VocabularyConfigInputModel model, CancellationToken cancellationToken = default);

    Task<Guid> AddVocabularyConfig(VocabularyConfigInputModel model, CancellationToken cancellationToken = default);

    Task DeleteVocabularyConfig(Guid id, CancellationToken cancellationToken = default);

    // TryGetVocabulary and BuildAllVocabulariesInCache are inherited from IVocabularyReader.
}
