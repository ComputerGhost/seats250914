using FluentValidation;

namespace Core.Application.Accounts;
public class VerifyPasswordCommandValidator : AbstractValidator<VerifyPasswordCommand>
{
    public VerifyPasswordCommandValidator()
    {
        RuleFor(p => p.Login)
            .NotEmpty();

        RuleFor(p => p.Password)
            .NotEmpty();
    }
}
