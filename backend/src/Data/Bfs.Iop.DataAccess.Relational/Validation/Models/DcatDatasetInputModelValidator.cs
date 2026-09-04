using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class DcatDatasetInputModelValidator : AbstractValidator<DcatDatasetInputModel>
{
    private static Guid? _idToUpdate = null;

    public DcatDatasetInputModelValidator(
        VocabularyEntryCodeValidator<RightsStatementsVocabulary> rightsStatementsValidator,
        VocabularyEntryCodeValidator<ConfidentialityPersonsVocabulary> confidentialityPersonValidator,
        VocabularyEntryCodeValidator<FrequencyTypesVocabulary> frequencyTypesValidator,
        VocabularyEntryCodeValidator<GeoIvIdsVocabulary> geoIvIdValidator,
        VocabularyEntryCodeValidator<Iso639LanguagesVocabulary> languagesValidator,
        VocabularyEntryCodeValidator<ThemesVocabulary> themesValidator,
        IValidator<ResourceModel> resourceInputModelValidator,
        IValidator<DcatQualifiedAttributionInputModel> dcatQualifiedAttributionInputModelValidator,
        IValidator<DcatQualifiedRelationInputModel> dcatQualifiedRelationInputModelValidator,
        IValidator<DcatDistributionInputModel> distributionInputModelValidator,
        IValidator<PeriodOfTimeModel> dateOnlyPeriodOfTimeValidator,
        IValidator<VCardModel> vCardInputModelValidator,
        IValidator<KeywordModel> keywordValidator,
        IopDbContext dbContext)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(_ => _)
            .Custom((model, context) =>
            {
                var idKey = ValidationContextDataKeys.IdKey;

                _idToUpdate = context.RootContextData.TryGetValue(idKey, out object? value)
                    ? (Guid)value
                    : null;
            });

        RuleFor(x => x.AccessRights.Code)
            .SetValidator(rightsStatementsValidator);

        RuleFor(x => x.ConfidentialityPerson!.Code)
            .SetValidator(confidentialityPersonValidator)
            .When(x => x.ConfidentialityPerson is not null);

        RuleForEach(x => x.ConformsTo)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.ContactPoints)
            .MustContainAtLeastOneItem()
            .DependentRules(() =>
            {
                RuleForEach(x => x.ContactPoints)
                    .SetValidator(vCardInputModelValidator);
            });

        RuleFor(x => x.DataOwner)
            .NotEmpty()
            .When(x => x.DataOwner is not null);

        RuleFor(x => x.Description)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleForEach(x => x.Distributions)
            .SetValidator(distributionInputModelValidator);

        RuleFor(x => x.Distributions)
            .Must(ds => ds.All(d => d.Id is null))
            .WithMessage("All distributions Ids must be null.")
            .When(_ => _idToUpdate is null);

        RuleForEach(x => x.Documentation)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.Frequency!.Code)
            .SetValidator(frequencyTypesValidator)
            .When(x => x.Frequency is not null);

        RuleFor(x => x.GeoIvIds)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.GeoIvIds.Select(x => x.Code))
                    .SetValidator(geoIvIdValidator)
                    .WithName(x => nameof(x.GeoIvIds));
            });

        RuleForEach(x => x.Identifiers)
            .MustBeValidIdentifier();

        RuleForEach(x => x.Identifiers)
            .Must((x, identifier) => x.Identifiers.Count(s => s == identifier) == 1)
            .WithMessage((_, identifier) => $"The identifier '{identifier}' must be unique within the collection.");

        RuleForEach(x => x.Identifiers)
            .Must((_, x) => !dbContext.Datasets.Any(y => y.Identifier.Contains(x) && y.Id != _idToUpdate))
            .WithMessage((_, x) => $"A resource with the identifier '{x}' already exists");

        RuleForEach(x => x.Images)
            .SetValidator(resourceInputModelValidator);

        RuleForEach(x => x.IsReferencedBy)
            .SetValidator(resourceInputModelValidator);

        RuleForEach(x => x.Keywords)
            .SetValidator(keywordValidator);

        RuleForEach(x => x.LandingPages)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.Languages)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.Languages.Select(x => x.Code))
                    .SetValidator(languagesValidator)
                    .WithName(x => nameof(x.Languages));
            });

        RuleFor(x => x.Publisher.Identifier)
            .NotEmpty();

        RuleForEach(x => x.QualifiedAttributions)
            .SetValidator(dcatQualifiedAttributionInputModelValidator);

        RuleForEach(x => x.QualifiedRelations)
            .SetValidator(dcatQualifiedRelationInputModelValidator);

        RuleForEach(x => x.Relations)
            .SetValidator(resourceInputModelValidator);

        RuleFor(_ => _)
            .Must(x => !string.Equals(x.ResponsibleDeputy!.Email, x.ResponsiblePerson!.Email, StringComparison.OrdinalIgnoreCase))
            .When(x => x.ResponsiblePerson is not null && x.ResponsibleDeputy is not null)
            .WithMessage(x => $"The {nameof(x.ResponsiblePerson)} and the {nameof(x.ResponsibleDeputy)} must be different.");

        RuleForEach(x => x.Spatial)
            .NotEmpty();

        RuleForEach(x => x.TemporalCoverage)
            .SetValidator(dateOnlyPeriodOfTimeValidator);

        RuleFor(x => x.Themes)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.Themes.Select(x => x.Code))
                    .SetValidator(themesValidator)
                    .WithName(x => nameof(x.Themes));
            });

        RuleFor(x => x.Title)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();
    }
}
