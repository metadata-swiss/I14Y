using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class DataServiceInputModelValidator : AbstractValidator<DataServiceInputModel>
{
    private static Guid? _idToUpdate = null;

    public DataServiceInputModelValidator(
        VocabularyEntryCodeValidator<RightsStatementsVocabulary> rightsStatementsValidator,
        VocabularyEntryCodeValidator<LicenseTypesVocabulary> licenseTypesValidator,
        VocabularyEntryCodeValidator<ThemesVocabulary> themesValidator,
        IValidator<ResourceModel> resourceInputModelValidator,
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

        RuleForEach(x => x.ConformsTo)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.ContactPoints)
            .MustContainAtLeastOneItem()
            .DependentRules(() =>
            {
                RuleForEach(x => x.ContactPoints)
                    .SetValidator(vCardInputModelValidator);
            });

        RuleFor(x => x.Description)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleForEach(x => x.Documentation)
            .SetValidator(resourceInputModelValidator);

        RuleForEach(x => x.EndpointDescriptions)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.EndpointUrls)
            .MustContainAtLeastOneItem()
            .DependentRules(() =>
            {
                RuleForEach(x => x.EndpointDescriptions)
                    .SetValidator(resourceInputModelValidator);
            });

        RuleForEach(x => x.Identifiers)
            .MustBeValidIdentifier();

        RuleForEach(x => x.Identifiers)
            .Must((x, identifier) => x.Identifiers.Count(s => s == identifier) == 1)
            .WithMessage((_, identifier) => $"The identifier '{identifier}' must be unique within the collection.");

        RuleForEach(x => x.Identifiers)
            .Must((_, x) => !dbContext.DataServices.Any(y => y.Identifiers.Contains(x) && y.Id != _idToUpdate))
            .WithMessage((_, x) => $"A resource with the identifier '{x}' already exists");

        RuleForEach(x => x.Keywords)
            .SetValidator(keywordValidator);

        RuleForEach(x => x.LandingPages)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.License!.Code)
            .SetValidator(licenseTypesValidator)
            .When(x => x.License is not null);

        RuleFor(x => x.Publisher.Identifier)
            .NotEmpty();

        RuleFor(_ => _)
            .Must(x => !string.Equals(x.ResponsibleDeputy!.Email, x.ResponsiblePerson!.Email, StringComparison.OrdinalIgnoreCase))
            .When(x => x.ResponsiblePerson is not null && x.ResponsibleDeputy is not null)
            .WithMessage(x => $"The {nameof(x.ResponsiblePerson)} and the {nameof(x.ResponsibleDeputy)} must be different.");

        RuleFor(x => x.ServesDatasets)
            .MustContainOnlyDistinctIds();

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
