using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Validation.Models;
using Bfs.Iop.DataAccess.Relational.Validation.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Validation.Services;

internal sealed class IopConceptsValidationService : IIopConceptsValidationService
{
    private readonly IopDbContext _dbContext;
    private readonly IValidator<IopConceptInputModel> _iopConceptInputModelValidator;
    private readonly IValidator<CodeListEntryInputModel> _codeListEntryInputModelValidator;
    private readonly IValidator<IEnumerable<CodeListEntryInputModel>> _codeListEntryInputModelsValidator;
    private readonly IValidator<IEnumerable<AnnotationInputModel>> _annotationInputModelsValidator;

    public IopConceptsValidationService(
        IopDbContext dbContext,
        IValidator<IopConceptInputModel> iopConceptInputModelValidator,
        IValidator<CodeListEntryInputModel> codeListEntryInputModelValidator,
        IValidator<IEnumerable<CodeListEntryInputModel>> codeListEntryInputModelsValidator,
        IValidator<IEnumerable<AnnotationInputModel>> annotationInputModelsValidator)
    { 
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        _iopConceptInputModelValidator = iopConceptInputModelValidator
            ?? throw new ArgumentNullException(nameof(iopConceptInputModelValidator));

        _codeListEntryInputModelValidator = codeListEntryInputModelValidator
            ?? throw new ArgumentNullException(nameof(codeListEntryInputModelValidator));

        _codeListEntryInputModelsValidator = codeListEntryInputModelsValidator 
            ?? throw new ArgumentNullException(nameof(codeListEntryInputModelsValidator));

        _annotationInputModelsValidator = annotationInputModelsValidator
            ?? throw new ArgumentNullException(nameof(annotationInputModelsValidator));
    }

    public void EnsureConceptCanBeAdded(IopConceptInputModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        var context = new ValidationContext<IopConceptInputModel>(inputModel);
        var result = _iopConceptInputModelValidator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    public void EnsureCodeListEntriesCanBeAdded(
        IEnumerable<CodeListEntryInputModel> inputModels,
        IopConcept conceptEntity,
        IEnumerable<CodeListEntry> codeListEntriesEntities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(conceptEntity, nameof(conceptEntity));
        ArgumentNullException.ThrowIfNull(codeListEntriesEntities, nameof(codeListEntriesEntities));

        var allCodes = inputModels
            .Select(x => x.Code)
            .Concat(codeListEntriesEntities.Select(x => x.Code))
            .ToList();

        var context = new ValidationContext<IEnumerable<CodeListEntryInputModel>>(inputModels);
        context.RootContextData[ValidationContextDataKeys.ConceptEntityKey] = conceptEntity;
        context.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = allCodes;
        var result = _codeListEntryInputModelsValidator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        // Check circular dependencies.
        // This code must be executed after the codes validation, in order to build the dictionaries properly.
        EnsureNoCircularParentCodeDependenciesInCodeListEntries(
            inputModels,
            codeListEntriesEntities);
    }

    public void EnsureCodeListEntryCanBeUpdated(
        Guid codeListEntryId,
        CodeListEntryInputModel updateModel,
        IopConcept conceptEntity,
        IEnumerable<CodeListEntry> codeListEntriesEntities)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));
        ArgumentNullException.ThrowIfNull(conceptEntity, nameof(conceptEntity));
        ArgumentNullException.ThrowIfNull(codeListEntriesEntities, nameof(codeListEntriesEntities));

        var allCodes = codeListEntriesEntities
            .Select(x => x.Code)
            .Prepend(updateModel.Code)
            .ToList();

        var context = new ValidationContext<CodeListEntryInputModel>(updateModel);
        context.RootContextData[ValidationContextDataKeys.ConceptEntityKey] = conceptEntity;
        context.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = allCodes;
        var result = _codeListEntryInputModelValidator.Validate(context);

        var codeListEntryWithSameCode = codeListEntriesEntities.SingleOrDefault(x => x.Code == updateModel.Code);

        if (codeListEntryWithSameCode is not null && codeListEntryWithSameCode.Id != codeListEntryId)
        {
            result.Errors.Add(
                new(nameof(updateModel.Code), $"A codelist entry with the code '{updateModel.Code}' already exists."));
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        // Check circular dependencies.
        // This code must be executed after the codes validation, in order to build the dictionaries properly.
        EnsureNoCircularParentCodeDependenciesInCodeListEntries(
            [updateModel],
            codeListEntriesEntities);
    }

    public void EnsureConceptCanBeUpdated(
        Guid id,
        IopConceptInputModel updateModel,
        IopConcept entity,
        IEnumerable<CodeListEntry> codeListEntryEntities)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(codeListEntryEntities, nameof(codeListEntryEntities));

        var context = new ValidationContext<IopConceptInputModel>(updateModel);

        context.RootContextData.Add(ValidationContextDataKeys.IdKey, id);

        var result = _iopConceptInputModelValidator.Validate(context);

        if (updateModel.ConceptType is ConceptType.CodeList && 
            entity.ConceptType is ConceptType.CodeList &&
            !CanCodeListConceptBeUpdated(updateModel, entity, codeListEntryEntities, out var errors))
        {
            result.Errors.AddRange(errors); 
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    public async Task EnsureConceptVersionCanBeAdded(
        Guid previousId,
        IopConceptInputModel inputModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        var result = new ValidationResult();

        var existingVersion = await _dbContext.IopConcepts.SingleOrDefaultAsync(i => i.Id == previousId, cancellationToken);
        
        if (existingVersion is null)
        {
            result.Errors.Add(
                new(
                    nameof(inputModel),
                    $"Previous version of the concept with id '{previousId}' was not found.")); 
        }
        else if (inputModel.Identifiers.Count() != existingVersion.Identifiers.Length ||
                 existingVersion.Identifiers.Except(inputModel.Identifiers).Any())
        {
            result.Errors.Add(
                new(
                    nameof(inputModel),
                    "The new version must have the same identifiers."));
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        EnsureConceptCanBeAdded(inputModel);
    }

    private static bool CanCodeListConceptBeUpdated(
        IopConceptInputModel updateModel, 
        IopConcept entity, 
        IEnumerable<CodeListEntry> codeListEntryEntities,
        out IReadOnlyList<ValidationFailure> failures)
    {
        var errors = new List<ValidationFailure>();

        if (updateModel.CodeListEntryValueType != entity.CodeListEntryValueType &&
            updateModel.CodeListEntryValueType is CodeListEntryValueType.Numeric)
        {
            var NanCode = codeListEntryEntities.FirstOrDefault(x => !decimal.TryParse(x.Code, out _))?.Code;

            if (NanCode is not null)
            {
                errors.Add(
                    new ValidationFailure(
                        nameof(updateModel.CodeListEntryValueType),
                        $"The value is not valid because there is at least one codelist entry with the code that is not a number (example:'{NanCode}')."));
            }
        }

        if (updateModel.CodeListEntryValueMaxLength < entity.CodeListEntryValueMaxLength)
        {
            var longCode = codeListEntryEntities.FirstOrDefault(x => x.Code.Length > updateModel.CodeListEntryValueMaxLength)?.Code;

            if (longCode is not null)
            {
                errors.Add(
                    new ValidationFailure(
                        nameof(updateModel.CodeListEntryValueMaxLength),
                        $"The value is not valid because there is at least one codelist entry with the code length greater than {updateModel.CodeListEntryValueMaxLength} (example:'{longCode}')."));
            }
        }

        failures = errors.AsReadOnly();
        return failures.Count is 0;
    }

    private static void EnsureNoCircularParentCodeDependenciesInCodeListEntries(
        IEnumerable<CodeListEntryInputModel> inputModels,
        IEnumerable<CodeListEntry> entities)
    {
        var inputCodesAndParentCodes = inputModels
            .ToDictionary(x => x.Code, x => x.ParentCode);

        var entitiesCodesAndParentCodes = entities
                .ToDictionary(x => x.Code, x => x.ParentCodeListEntry?.Code);

        var allCodesAndParentCodes = inputCodesAndParentCodes
            .Concat(entitiesCodesAndParentCodes.Where(x => !inputCodesAndParentCodes.ContainsKey(x.Key)))
            .ToDictionary();

        foreach (var pair in inputCodesAndParentCodes.Where(x => x.Value is not null))
        {
            var parentEntry = allCodesAndParentCodes[pair.Value!];

            while (parentEntry is not null)
            {
                if (parentEntry == pair.Key)
                {
                    throw new ValidationException(
                        $"Circular parent dependency detected when setting parent code '{pair.Value}' in code list entry '{pair.Key}'.");
                }

                parentEntry = allCodesAndParentCodes[parentEntry];
            }
        }
    }
}
