using Bfs.Iop.Core.Abstractions.Models;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class CodeListEntryInputModelsValidator : AbstractValidator<IEnumerable<CodeListEntryInputModel>>
{
    public CodeListEntryInputModelsValidator(IValidator<CodeListEntryInputModel> validator)
    {
        RuleForEach(x => x).SetValidator(validator);

        // Validate repeated codes
        // 1. Check for repeated codes in input models
        // 2. Check for repeated codes in database
        RuleFor(x => x).Custom((x, context) =>
        {
            var inputCodes = x.Select(x => x.Code);

            var repeatedCodes = inputCodes
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count())
                .Where(x => x.Value > 1)
                .Select(x => x.Key);

            if (repeatedCodes.Any())
            {
                context.AddFailure($"The codelist entries provided have repeated codes: {string.Join(", ", repeatedCodes)}.");
            }
            else
            {
                var allCodeListEntryCodesKey = ValidationContextDataKeys.AllCodeListEntriesCodesKey;
                var allCodes = (IEnumerable<string>)context.RootContextData[allCodeListEntryCodesKey];

                repeatedCodes = allCodes
                    .GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count())
                    .Where(x => x.Value > 1)
                    .Select(x => x.Key);

                if (repeatedCodes.Any())
                {
                    context.AddFailure($"Codelist entries already exist with the following codes: {string.Join(", ", repeatedCodes)}.");
                }
            }
        });
    }
}
