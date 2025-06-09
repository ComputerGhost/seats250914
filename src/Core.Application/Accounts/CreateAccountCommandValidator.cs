using FluentValidation;

namespace Core.Application.Accounts;
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(p => p.Login)
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(p => p.Password)
            .NotEmpty()
            // We don't care here about the max length here,
            // but the presentation layer might want to limit it.
            .MaximumLength(int.MaxValue);
    }
}
