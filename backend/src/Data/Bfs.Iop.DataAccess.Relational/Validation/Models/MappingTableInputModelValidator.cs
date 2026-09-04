using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class MappingTableInputModelValidator : AbstractValidator<MappingTableInputModel>
{
    private static Guid? _idToUpdate = null;

    public MappingTableInputModelValidator(
        IopDbContext dbContext,
        IValidator<ResourceModel> resourceModelValidator,
        IValidator<KeywordModel> keywordValidator,
        VocabularyEntryCodeValidator<ThemesVocabulary> themesValidator)
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

        RuleForEach(x => x.ConformsTo)
            .SetValidator(resourceModelValidator);

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
                return dbContext.MappingTables
                    .Where(x => x.Identifiers.Contains(identifier))
                    .Include(x => x.Publisher)
                    .Select(x => x.Publisher.Identifier)
                    .FirstOrDefault(x => x != model.Publisher.Identifier) is null;
            })
            .WithMessage((_, identifier) => $"The identifier '{identifier}' already exists by another publisher.");

        RuleForEach(x => x.Identifiers)
            .Must((model, identifier) =>
                dbContext.MappingTables.All(x => !(x.Identifiers.Contains(identifier) && x.Version == model.Version) || x.Id == _idToUpdate))
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

        RuleFor(x => x.Source.Uri)
            .Must(uri => uri.IsValidUri())
            .WithMessage((_, uri) => $"'{uri}' is not a valid uri.");

        RuleFor(x => x.Target.Uri)
            .Must(uri => uri.IsValidUri())
            .WithMessage((_, uri) => $"'{uri}' is not a valid uri.");

        RuleFor(x => x.Themes)
           .MustContainOnlyDistinctCodes()
           .DependentRules(() =>
           {
               RuleForEach(x => x.Themes.Select(x => x.Code))
                   .SetValidator(themesValidator)
                   .WithName(x => nameof(x.Themes));
           });

        RuleFor(x => x.ValidTo)
            .Must((x, d) => x.ValidFrom!.Value.CompareTo(x.ValidTo!.Value) <= 0)
            .When(x => x.ValidTo.HasValue && x.ValidFrom.HasValue)
            .WithMessage(x => $"{nameof(x.ValidTo)} must not be earlier than {nameof(x.ValidFrom)}.");

        RuleFor(x => x.Version)
            .Must(ver => ver.IsValidVersion())
            .WithMessage((_, x) => $"The value '{x}' does not match the semantic version pattern.");
    }
}
