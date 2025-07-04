using FluentValidation;

namespace Core.Application.Reservations;
public class ReserveSeatCommandValidator : AbstractValidator<ReserveSeatCommand>
{
    public ReserveSeatCommandValidator()
    {
        RuleFor(p => p.Email)
            .MaximumLength(255)
            .NotEmpty();

        RuleFor(p => p.IpAddress)
            .MaximumLength(45)
            .NotEmpty();

        RuleFor(p => p.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(15);

        RuleFor(p => p.PreferredLanguage)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(p => p.SeatKey)
            .Length(44)
            .NotEmpty();
    }
}
