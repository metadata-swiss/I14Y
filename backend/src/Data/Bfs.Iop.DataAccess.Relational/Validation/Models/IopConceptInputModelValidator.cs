using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class IopConceptInputModelValidator : AbstractValidator<IopConceptInputModel>
{
    private static Guid? _idToUpdate = null;

    public IopConceptInputModelValidator(
        VocabularyEntryCodeValidator<ThemesVocabulary> themesValidator,
        IValidator<ResourceModel> resourceModelValidator,
        IValidator<KeywordModel> keywordValidator,
        IopDbContext dbContext)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(_ => _)
          .Custom((model, context) =>
          {
              var idKey = ValidationContextDataKeys.IdKey;

              _idToUpdate = context.RootContextData.TryGetValue(idKey, out object? value)
                  ? (global::System.Guid)value
                  : null;
          });

        RuleFor(x => x.ConceptType)
            .IsInEnum();

        When(x => x.ConceptType is ConceptType.CodeList, SetRulesForCodeListConcept);
        When(x => x.ConceptType is ConceptType.Date, SetRulesForDateConcept);
        When(x => x.ConceptType is ConceptType.Numeric, SetRulesForNumericConcept);
        When(x => x.ConceptType is ConceptType.String, SetRulesForStringConcept);

        RuleForEach(x => x.ConformsTo)
            .SetValidator(resourceModelValidator);

        RuleForEach(x => x.Replaces)
            .Must((model, r) => r.Id != _idToUpdate)
            .WithMessage("A concept cannot replace itself.")
            .Must(r => dbContext.IopConcepts.Any(c => c.Id == r.Id))
            .WithMessage((_, r) => $"The concept with id '{r.Id}' does not exist on I14Y.");

        RuleFor(x => x.Replaces)
            .Must(replaces => replaces.DistinctBy(r => r.Id).Count() == replaces.Count())
            .WithMessage("The 'Replaces' entries must reference distinct concepts.");

        RuleFor(x => x.Description)
            .NotNull()
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleForEach(x => x.Identifiers)
            .MustBeValidIdentifier();

        RuleForEach(x => x.Identifiers)
            .Must((x, identifier) => x.Identifiers.Count(s => s == identifier) == 1)
            .WithMessage((_, identifier) => $"The identifier '{identifier}' must be unique within the collection.");

        RuleForEach(x => x.Identifiers)
            .Must((model, identifier) =>
            {
                return dbContext.IopConcepts
                    .Where(x => x.Identifiers.Contains(identifier))
                    .Include(x => x.Publisher)
                    .Select(x => x.Publisher.Identifier)
                    .FirstOrDefault(x => x != model.Publisher.Identifier) is null;
            })
            .WithMessage((_, identifier) => $"The identifier '{identifier}' already exists by another publisher.");

        RuleForEach(x => x.Identifiers)
            .Must((model, identifier) =>
                dbContext.IopConcepts.All(x => !(x.Identifiers.Contains(identifier) && x.Version == model.Version) || x.Id == _idToUpdate))
            .WithMessage((model, identifier) => $"The identifier '{identifier}' with version '{model.Version}' is already in use.");

        RuleForEach(x => x.Keywords)
            .SetValidator(keywordValidator);

        RuleFor(x => x.Name)
            .NotNull()
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleFor(x => x.ResponsiblePerson)
            .NotNull();

        RuleFor(_ => _)
            .Must(x => !string.Equals(x.ResponsibleDeputy!.Email, x.ResponsiblePerson.Email, StringComparison.OrdinalIgnoreCase))
            .When(x => x.ResponsibleDeputy is not null && x.ResponsiblePerson is not null)
            .WithMessage(x => $"The {nameof(x.ResponsiblePerson)} and the {nameof(x.ResponsibleDeputy)} must be different.");

        RuleFor(x => x.Themes)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.Themes.Select(x => x.Code))
                    .SetValidator(themesValidator)
                    .WithName(x => nameof(x.Themes));
            });

        RuleFor(x => x.ValidTo)
            .Must((model, validTo) => model.ValidFrom!.Value <= validTo!.Value)
            .When(x => x.ValidTo.HasValue && x.ValidFrom.HasValue)
            .WithMessage(x => $"{nameof(x.ValidTo)} must not be earlier than {nameof(x.ValidFrom)}.");

        RuleFor(x => x.Version)
            .Must(ve => ve.IsValidVersion())
            .WithMessage((_, x) => $"The value '{x}' does not match the semantic version pattern.");
    }

    private void SetRulesForCodeListConcept()
    {
        RuleFor(x => x.CodeListEntryValueMaxLength)
            .NotNull()
            .GreaterThan(0);

        RuleFor(x => x.CodeListEntryValueType)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.CodeListEntryDefaultSortProperty)
            .IsInEnum()
            .When(x => x.CodeListEntryDefaultSortProperty.HasValue);
    }

    private void SetRulesForNumericConcept()
    {
        RuleFor(x => x.MinValue)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.MaxValue)
                    .NotNull()
                    .Must((x, _) => x.MaxValue >= x.MinValue)
                    .WithMessage(x => $"{nameof(x.MaxValue)} must be equal to or greater than {nameof(x.MinValue)}.");
            });

        RuleFor(x => x.NumberDecimals)
            .NotNull()
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Pattern)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.Pattern is not null);
    }

    private void SetRulesForStringConcept()
    {
        RuleFor(x => x.MinLength)
            .NotNull()
            .GreaterThanOrEqualTo(0)
            .DependentRules(() =>
            {
                RuleFor(x => x.MaxLength)
                    .NotNull()
                    .Must((x, _) => x.MaxLength >= x.MinLength)
                    .WithMessage(x => $"{nameof(x.MaxLength)} must be equal to or greater than {nameof(x.MinLength)}.");
            });

        RuleFor(x => x.Pattern)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.Pattern is not null);
    }

    private void SetRulesForDateConcept()
    {
        RuleFor(x => x.Pattern)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.Pattern is not null);
    }
}
