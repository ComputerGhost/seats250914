using FluentValidation;

namespace Core.Application.Accounts;
public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(p => p.Login)
            .NotEmpty();
    }
}
