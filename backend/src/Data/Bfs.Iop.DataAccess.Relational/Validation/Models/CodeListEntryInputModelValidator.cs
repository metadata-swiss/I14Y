using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class CodeListEntryInputModelValidator : AbstractValidator<CodeListEntryInputModel>
{
    public CodeListEntryInputModelValidator(
        IValidator<AnnotationInputModel> annotationInputModelValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        SetRulesForCodeProperty();

        SetRulesForParentCodeProperty();

        RuleFor(model => model.Annotations)
            .ForEach(annotation => annotation.SetValidator(annotationInputModelValidator));

        When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue, 
            () =>
                RuleFor(_ => _)
                    .Must(x => x.ValidFrom!.Value.CompareTo(x.ValidTo!.Value) <= 0)
                    .WithMessage("ValidFrom must be earlier than or equal to ValidTo value."));
    }

    private void SetRulesForCodeProperty()
    {
        RuleFor(model => model.Code)
            .Must(val => !val.IsNullOrEmptyOrContainsWhiteSpace())
            .WithMessage("The value cannot be null, empty or contain white spaces.");

        RuleFor(model => model).Custom((x, context) =>
        {
            var conceptKey = ValidationContextDataKeys.ConceptEntityKey;

            var concept = (IopConcept)context.RootContextData[conceptKey];

            if (concept.CodeListEntryValueType is CodeListEntryValueType.Numeric)
            {
                if (!decimal.TryParse(x.Code, out var codeValue))
                {
                    context.AddFailure(nameof(x.Code), "The value must be a number.");
                }

                var isCodeAnInt = codeValue % 1m == 0m;

                if (x.Code.Length > 1 && isCodeAnInt && x.Code.StartsWith('0'))
                {
                    context.AddFailure(nameof(x.Code), "Integer values different from '0' cannot start with '0'.");
                }

                if (!isCodeAnInt && x.Code.EndsWith('0'))
                {
                    context.AddFailure(nameof(x.Code), "Decimal values cannot end with '0'.");
                }
            }
            
            if (x.Code.Length > concept.CodeListEntryValueMaxLength)
            {
                context.AddFailure(nameof(x.Code), $"The value length cannot be greater than {concept.CodeListEntryValueMaxLength} chars.");
            }
        });
    }

    private void SetRulesForParentCodeProperty()
    {
        RuleFor(model => model.ParentCode)
            .Must(x => !x.IsNullOrEmptyOrContainsWhiteSpace())
            .When(model => model.ParentCode is not null)
            .WithMessage("The value cannot be empty or contain white spaces.");

        RuleFor(model => model.ParentCode)
            .Must((x, _) => x.Code != x.ParentCode)
            .When(x => x.ParentCode is not null)
            .WithMessage("The parent code must be a different codelist entry.");

        RuleFor(model => model).Custom((x, context) =>
        {
            var allCodeListEntriesCodesKey = ValidationContextDataKeys.AllCodeListEntriesCodesKey;

            var allCodeListEntriesCodes = (IEnumerable<string>)context.RootContextData[allCodeListEntriesCodesKey];

            if (x.ParentCode is not null && !allCodeListEntriesCodes.Any(c => c == x.ParentCode))
            {
                context.AddFailure(nameof(x.ParentCode), $"The code '{x.ParentCode}' was not found in the code list.");
            }
        });
    }
}
