using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class ExportConceptCommandHandler : IRequestHandler<ExportConceptCommand, ExportFile>
{
    private readonly IIopConceptsService _iopConceptsService;

    public ExportConceptCommandHandler(IIopConceptsService iopConceptsService) =>
        _iopConceptsService = iopConceptsService;

    public async Task<ExportFile> Handle(ExportConceptCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFormat is not DataFormat.Json)
        {
            throw new NotSupportedException($"Data format {request.DataFormat} is not supported for concept export.");
        }

        var concept = await _iopConceptsService.GetIopConcept(request.ConceptId, cancellationToken);

        var entries = concept.ConceptType is ConceptType.CodeList ?
            await _iopConceptsService.GetCodeListEntries(
                request.ConceptId,
                concept.CodeListEntryDefaultSortProperty ?? default,
                sortOrder: default,
                page: 1,
                pageSize: int.MaxValue, cancellationToken)
            : null;

        var filename = $"Concept_{concept.Identifiers.First()}-{concept.Version}";

        return IopJsonSerializer.SerializeToFile(filename, MapToExportModel(concept, entries?.Results));
    }

    private static IopConceptExportModel MapToExportModel(
        IopConceptModel concept,
        IReadOnlyCollection<CodeListEntryModel>? codeListEntries) =>
    new(codeListEntries)
    {
        CodeListEntryDefaultSortProperty = concept.CodeListEntryDefaultSortProperty,
        CodeListEntryValueMaxLength = concept.CodeListEntryValueMaxLength,
        CodeListEntryValueType = concept.CodeListEntryValueType,
        ConceptType = concept.ConceptType,
        ConformsTo = concept.ConformsTo,
        Description = concept.Description,
        Id = concept.Id,
        Identifiers = concept.Identifiers,
        IsLocked = concept.IsLocked,
        Keywords = concept.Keywords,
        MaxLength = concept.MaxLength,
        MaxValue = concept.MaxValue,
        MeasurementUnit = concept.MeasurementUnit,
        MinLength = concept.MinLength,
        MinValue = concept.MinValue,
        Name = concept.Name,
        NumberDecimals = concept.NumberDecimals,
        Pattern = concept.Pattern,
        Replaces = concept.Replaces,
        IsReplacedBy = concept.IsReplacedBy,
        PublicationLevel = concept.PublicationLevel,
        PublicationLevelProposal = concept.PublicationLevelProposal,
        Publisher = concept.Publisher,
        RegistrationStatus = concept.RegistrationStatus,
        RegistrationStatusProposal = concept.RegistrationStatusProposal,
        ResponsibleDeputy = concept.ResponsibleDeputy,
        ResponsiblePerson = concept.ResponsiblePerson,
        System = concept.System,
        Themes = concept.Themes,
        ValidFrom = concept.ValidFrom,
        ValidTo = concept.ValidTo,
        Version = concept.Version
    };

    private sealed record IopConceptExportModel(IReadOnlyCollection<CodeListEntryModel>? CodeListEntries) : IopConceptModel
    { }
}
