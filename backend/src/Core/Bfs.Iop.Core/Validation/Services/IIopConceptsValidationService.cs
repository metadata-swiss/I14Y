using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Validation.Services;

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