using FluentValidation;

namespace Core.Application.Reservations;
public class UpdateReservationCommandValidator : AbstractValidator<UpdateReservationCommand>
{
    public UpdateReservationCommandValidator()
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
