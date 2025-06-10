using FluentValidation;

namespace Core.Application.Accounts;
public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(p => p.Login)
            .NotEmpty();

        RuleFor(p => p.Password)
            .NotEmpty()
            // We don't care here about the max length here,
            // but the presentation layer might want to limit it.
            .MaximumLength(int.MaxValue);
    }
}
