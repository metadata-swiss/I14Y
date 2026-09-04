using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

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
