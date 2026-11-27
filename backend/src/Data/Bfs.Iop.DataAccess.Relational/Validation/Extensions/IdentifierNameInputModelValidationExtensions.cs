using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Extensions;

internal static class IdentifierNameInputModelValidationExtensions
{
    public static IRuleBuilderOptions<T, IdentifierNameInputModel> MustBeValidPublisherInput<T>(
        this IRuleBuilder<T, IdentifierNameInputModel> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .Must(x => !string.IsNullOrWhiteSpace(x.Identifier) || (!x.Name?.IsContentNullOrWhiteSpace() ?? false))
            .WithMessage(_ => "The publisher must have either a valid identifier or a valid name.");
    }

    public static IRuleBuilderOptions<T, IdentifierNameInputModel> MustBeValidAgentInput<T>(
        this IRuleBuilder<T, IdentifierNameInputModel> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .Must(x => !string.IsNullOrWhiteSpace(x.Identifier) || (!x.Name?.IsContentNullOrWhiteSpace() ?? false))
            .WithMessage(_ => "The agent must have either a valid identifier or a valid name.");
    }
}
