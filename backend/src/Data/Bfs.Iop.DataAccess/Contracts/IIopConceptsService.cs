using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

public interface IIopConceptsService : IPublishableEntityService
{
    Task<IopConceptModel> GetIopConcept(
        Guid id, 
        bool includeCodeListEntries,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<IopConceptModel>> GetIopConcepts(
        IEnumerable<Guid> ids, 
        CancellationToken cancellationToken = default);

    Task<PagedResult<IopConceptModel>> GetIopConcepts(
        string? conceptIdentifier,
        string? publisherIdentifier,
        string? version,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IdentifierVersionExistsModel> GetIdentifierVersionExists(
        string identifier, 
        string version, 
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<IopConceptModel>> GetIopConceptsForIndexInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    Task<CodeListEntryModel> GetCodeListEntry(
        Guid conceptId,
        Guid codeListEntryId,
        CancellationToken cancellationToken = default);

    Task<CodeListEntryModel> GetCodeListEntryByCode(
        Guid conceptId,
        string code,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CodeListEntryModel>> GetCodeListEntries(
        Guid conceptId,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CodeListEntryModel>> GetCodeListEntriesByRoot(
        Guid conceptId,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CodeListEntryModel>> GetCodeListEntriesChildrenOfParentCode(
        Guid conceptId,
        string parentCode,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> GetCodeListEntriesHasParentCodes(
        Guid conceptId,
        CancellationToken cancellationToken = default);

    Task<int> GetCodeListEntriesPageNumberFromSameParent(
        Guid conceptId,
        string codeValue,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CodeListEntryModel>> GetCodeListEntriesForAutoComplete(
        Guid conceptId,
        string codePrefix,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<List<CodeListEntryModel>> GetCodeListEntriesForIndexInBatches(int batchSize = 100, CancellationToken cancellationToken = default);

    Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesByIds(IEnumerable<Guid> codeListEntryIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesByCodes(Guid conceptId, IEnumerable<string> codeListEntryCodes, CancellationToken cancellationToken = default);

    Task<Guid> AddIopConcept(IopConceptInputModel inputModel, CancellationToken cancellationToken = default);

    Task<Guid> AddIopConceptVersion(
        Guid previousId,
        IopConceptInputModel inputModel,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Guid>> AddCodeListEntries(
        Guid conceptId,
        IEnumerable<CodeListEntryInputModel> inputModels,
        CancellationToken cancellationToken = default);

    Task UpdateConcept(Guid id, IopConceptInputModel updateModel, CancellationToken cancellationToken = default);

    Task UpdateCodeListEntry(
        Guid conceptId,
        Guid codeListEntryId,
        CodeListEntryInputModel updateModel,
        CancellationToken cancellationToken = default);

    Task UpdateIsLocked(Guid id, bool value, CancellationToken cancellationToken = default);

    Task DeleteIopConcept(Guid id, CancellationToken cancellationToken = default);

    Task DeleteCodeListEntry(Guid conceptId, Guid codeListEntryId, CancellationToken cancellationToken = default);

    Task DeleteAllCodeListEntriesFromIopConcept(Guid conceptId, CancellationToken cancellationToken = default);
    
}