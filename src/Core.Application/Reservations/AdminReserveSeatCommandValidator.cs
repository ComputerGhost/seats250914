using FluentValidation;

namespace Core.Application.Reservations;
public class AdminReserveSeatCommandValidator : AbstractValidator<AdminReserveSeatCommand>
{
    public AdminReserveSeatCommandValidator()
    {
        RuleFor(p => p.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(p => p.Email)
            .MaximumLength(255)
            .NotEmpty();

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(15);

        RuleFor(p => p.PreferredLanguage)
            .MaximumLength(50)
            .NotEmpty();
    }
}
