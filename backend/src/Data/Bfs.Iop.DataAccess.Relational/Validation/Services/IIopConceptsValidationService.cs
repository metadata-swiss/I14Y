using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Validation.Services;

internal interface IIopConceptsValidationService
{
    void EnsureConceptCanBeAdded(IopConceptInputModel inputModel);

    void EnsureCodeListEntriesCanBeAdded(
        IEnumerable<CodeListEntryInputModel> inputModels,
        IopConcept conceptEntity,
        IEnumerable<CodeListEntry> codeListEntriesEntities);

    void EnsureConceptCanBeUpdated(
        Guid id,
        IopConceptInputModel updateModel,
        IopConcept entity,
        IEnumerable<CodeListEntry> codeListEntryEntities);

    void EnsureCodeListEntryCanBeUpdated(
        Guid codeListEntryId,
        CodeListEntryInputModel updateModel,
        IopConcept conceptEntity,
        IEnumerable<CodeListEntry> codeListEntriesEntities);

    Task EnsureConceptVersionCanBeAdded(
        Guid previousId,
        IopConceptInputModel inputModel,
        CancellationToken cancellationToken);
}