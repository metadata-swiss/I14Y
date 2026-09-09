using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Extensions;

internal static class MultiLanguageModelValidationExtensions
{
    public static IRuleBuilderOptions<T, MultiLanguageModel> MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace<T>(
        this IRuleBuilder<T, MultiLanguageModel> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .NotNull()
            .Must(x => !x.IsContentNullOrWhiteSpace())
            .WithMessage(_ => "At least one language must be different from null, empty or white space.");
    }
}
