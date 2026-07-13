using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class VCardInputModelValidator : AbstractValidator<VCardModel>
{
    public VCardInputModelValidator()
    {
        RuleFor(x => x.HasEmail)
            .Must(email => email.IsValidEmail())
            .WithMessage("The value is not a valid email.");

        RuleFor(x => x.Kind)
            .IsInEnum();
    }
}
